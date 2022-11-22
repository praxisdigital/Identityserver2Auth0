// See https://aka.ms/new-console-template for more information
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;



var result = await ImportUsersAsync(new List<IdentityUser>() {

    // Just for testing, you should get the users from you database
    new IdentityUser()
    {
        Id = 1,
        Email = "email@domain.com",
        EmailConfirmed = true,
        Name = "Firstname LastName",
        PasswordHash = "AQAAAAEAACcQAAAAEN8Zdgoj2TaTXrdjAphMGMnSlWA5kAJ63VqxmN84C3S8e8TushPTuBozpOrBnwtavg=="
    } });

// obs their is a 500kb limit on file size when importing into Auth0, you might whant to split into multiple files if you have a lot of users
File.WriteAllText("Auth0Users.json", result);

Console.WriteLine("DONE");


async Task<string> ImportUsersAsync(List<IdentityUser> users)
{
    List<Auth0User> usersToImport = new List<Auth0User>();

    foreach (var user in users)
    {
        var auth0User = new Auth0User()
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            Name = user.Name,
            //Name = (string.IsNullOrEmpty(user.Name) ? user.Email : user.Name + (string.IsNullOrEmpty(user.Surname) ? "" : " " + user.Surname)),
            EmailConfirmed = user.EmailConfirmed
        };



        // Convert the stored Base64 password to bytes
        byte[] decodedPasswordHash = Convert.FromBase64String(user.PasswordHash);

        int identityVersion = decodedPasswordHash[0];
        byte[] byteArray = new byte[4];

        Array.Copy(decodedPasswordHash, 1, byteArray, 0, 4);
        Array.Reverse(byteArray);
        uint key = BitConverter.ToUInt32(byteArray);

        Array.Copy(decodedPasswordHash, 5, byteArray, 0, 4);
        Array.Reverse(byteArray);
        uint iterations = BitConverter.ToUInt32(byteArray);

        Array.Copy(decodedPasswordHash, 9, byteArray, 0, 4);
        Array.Reverse(byteArray);
        uint saltSize = BitConverter.ToUInt32(byteArray);

        byteArray = new byte[16];
        Array.Copy(decodedPasswordHash, 13, byteArray, 0, 16);
        string salt = Convert.ToBase64String(byteArray).Replace('=', ' ').Trim(' ');

        byteArray = new byte[32];
        Array.Copy(decodedPasswordHash, 29, byteArray, 0, 32);
        string hash = Convert.ToBase64String(byteArray).Replace('=', ' ').Trim(' ');


        Array.Reverse(byteArray);

        var pbkdf2String = ($"$pbkdf2-sha256$i={iterations},l=32${salt}${hash}");

        auth0User.CustomPasswordHash = new CustomPasswordHash()
        {
            Algorithm = "pbkdf2",
            Hash = new Hash()
            {
                Value = pbkdf2String,
                Encoding = "utf8"
            }
        };

        usersToImport.Add(auth0User);
    }

    var jsonContent = JsonSerializer.Serialize(usersToImport);

    return jsonContent;
}

public class IdentityUser
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string PasswordHash { get; set; }
}

public class Hash
{
    [JsonPropertyName("value")]
    public string Value { get; set; }
    [JsonPropertyName("encoding")]
    public string Encoding { get; set; }
}


public class CustomPasswordHash
{
    [JsonPropertyName("algorithm")]
    public string Algorithm { get; set; }
    [JsonPropertyName("hash")]
    public object Hash { get; set; }
}

public class Auth0User
{
    [JsonPropertyName("user_id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("email_verified")]
    public bool EmailConfirmed { get; set; }

    [JsonPropertyName("custom_password_hash")]
    public CustomPasswordHash CustomPasswordHash { get; set; }
}