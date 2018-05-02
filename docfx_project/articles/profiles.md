---
uid: profiles.md
title: Profiles
---

# Profiles

Profiles are used to selectively apply migrations depending on a command line option, or build script attribute. For instance, you may want to run the "Development" profile only in your Dev environment, and run the "Testing" profile in your Testing environment. Profiles will *always* be run if you specify them.

Normally, these are useful to always put test/sample seed data in when dropping/refreshing development and testing environments. In production you would rarely want to continually insert the same data more than once.

```cs
[Profile("Development")]
public class CreateDevSeedData : Migration
{
    public override void Up()
    {
        Insert.IntoTable( "User" ).Row( new
            {
                Username = "devuser1",
                DisplayName = "Dev User"
            });
    }

    public override void Down()
    {
        //empty, not using
    }
}
```

Now that you have migrations and profiles defined, you can use [Migration Runners](migration-runners.md) to apply changes to your database.
