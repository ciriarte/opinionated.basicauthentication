## opinionated.basicauthentication

Adds Basic Authentication To AspNet Owin Projects

### Usage:

```csharp
public void Configuration(IAppBuilder app)
{
  app.UseBasicAuthentication("westeros", Validate);
  // Other middleware (make sure it is bound before others, order matters)
}

Task<IEnumerable<Claim>> Validate(string id, string secret)
{
    // You can use anything you want here, to validate the user and build your claims.
    // I am using a member userManager (defined somewhere else) to fetch my user from
    // a db, STS, etc.
    
    var incoming = userManager.Find(id, secret);

    return null != incoming ? Task.FromResult(incoming.Claims) 
                            : Task.FromResult<IEnumerable<Claim>>(null);
}

```

![fancy pants](https://cloud.githubusercontent.com/assets/5928/8072144/c0605708-0ec6-11e5-98d9-b513d342c905.jpg)
