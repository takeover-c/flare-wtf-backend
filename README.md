# Flare.Backend
Backend of Flare application developed for 4th Serres Hackathon 2019.

## About

The backend is written in C# and .NET core 2.2. It provides REST APIs to be consumed by Flare frontend application and some automation tools (e.g. crons that automatically uploads log files perodically).

## Requirements

.NET Core 2.2 SDK and Runtime tools.
Visual Studio 2017+ with .NET Core tooling installed (or JetBrains Rider if you prefer to code on Linux or MacOS)
MySQL Server 8 (or compatible MariaDB version)

## Install

Create appsettings.json on src\Flare.Backend folder with customized settings.

```js
{
  // put your database id/pw here
  "database": "Server=localhost;Database=flare;User=flare;Password=flare;",
  // put a RSA key encoded in XML format here. It is used to validate and sign the JWT tokens.
  "jwt": "<RSAKeyValue><Modulus>y6bYyNAfMC6zS35sJxgBeoUdKifYWREw9FjveLGBOQYUT7szdNkIV7ELnnjt6McauCxRzExMrajtocelLmKp1gr9WdkIahzKS082Rb7DSCGFvFOfTRB85Yo97npT1SF3SUF94AOr+3qKIZ3ePTYLld+fanq4WzehmAcwEBJEN2JSq4wC3cbhUD1FguIV5Fz250tWi0+K4iNs/ZA82lYUOTmbwz/XBTtYe8viYvfN+GQFFZtVD++2/FLbNFNmOTDQ9oLxVQcYFPt2ob7PQUqbCV/3VbyVpOsRcsAGTLPNm3Gny8SmZHP9ijHi+3HcfQpsXHY1Dcbq13BgmXw7zTXnHw==</Modulus><Exponent>AQAB</Exponent><P>4XvYcMph4n3BwDdK9kkugiWY6dHxz8cEZf/egCHcYGGoKTYncTd7pBhYBLNpkyppmeGjD/JL2F/ITHn2ENN1YGM/SAzJ6G3fm7O3zI+KRki7d/Tx4NRw0ukjDlEsXjnZ+qu/ec0y6XaniIDZ5UZPBjHGx/wlCzEdKrofgWa6Imk=</P><Q>5zaaz6Gi1SDls3f3Cn2Ts16tN7TEsLdMAWYH2ayDoaLHi6VfexserzeUd050dORaUuUOoxKulPb562MeUKxK4qrC/girf+DRbGrXNih+XOcgyjZSMrGVcz8hSEgf7nhO2SBTJ6cbVEk7aaVvzPeeZQBR/Dt+AeutNlyPtDMf/Ec=</Q><DP>VbKIFHYWtcot9SwOpYQy1OwNLfDJArsVBN82ZWR5KXh0PRWD8BeyOi+2ZDL8ER6xe/axzsG76cSdj9NanhKALd4KkwupQVlg/wBS/sAfQY8rEPBbbaPQLZjF5g8b2cQLAKZ944EFtI2QJuUx613JcwVAE4nVWmeUOkT67GdqnuE=</DP><DQ>wgkSPINCWqgj/MwFdzlRtasTpPdARald0KKmnedoBELOQYREL9TfmF4Fa2Zc1yg4IO73rKTl/D+UFxV0gHFG2xhBUd1Gx6eNOPYkq9+pxB93ZhvWOZLMgA4PyVA18/Pk/9Rf1JwplD9s68kZnSKFO+/b3dizc7sr/r4JH0n0Yss=</DQ><InverseQ>D76Zx8oh+3NwNIjY3Ijz1D5OtNXQvHD7dAxae6V7IVHLAhyBBeNmCLJEj1OZwYZ/phTKkU/ORbSmaRCAklgdzkrSpxHEoWrehB2PxQUtKnTQTef2kWGs4aHHDVeMKRJGIgQEgKEFr+t3XC1GP17jKOCC9YwjZFACxk01W8o8bmg=</InverseQ><D>DvmGyUVcdbuJXYy/QSSnvy2Ylmf1pZ74/Y6olTRgLyQ+TBqUzwvCbEhNdWqo+8vgMBi6Lw6RONfKGXJHuCBzIpUOShQGZV8WlPEPKZO8YkpsvgtcFFuZA3vlz+pYzbw3PEC/k8BRXh2FmLE2DUQzF+MoorjBY5u0myw0IYpmT7gejR1/9sNzM2az1h7YTqP5gmF4o4zpUavLpnpMgq2maxFJlFJpl5b1fWNg8NTnU6V9BBhz3OBqeO0oxZqRq10WYjlL3lHFHgyNEcJrBNC/+9OOd8ACuWjTdfegVM3QkdbUF+IsMMhl0Od7GAFrEdb9JfNW/7gFbtsT7bdLtG0UUQ==</D></RSAKeyValue>",
  // put a random string here, it is used to restrict access to uploaded files.
  "hash": "Flare2019",
  // put a SMTP server and credientals here, it is used to send e-mails.
  "smtp": {
    "server": "mail.google.com",
    "port": 587,
    "user": "sample-email-account@gmail.com",
    "pass": "ChangeMeForSure"
  }
}
```

And follow the following documentation provided by Microsoft to publish the application to a server:

[Host and deploy ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/?view=aspnetcore-2.2)

## Usage

APIs will be provided by the end of the hackathon event.

## Live demo

Please check [takeover-c/flare-wtf-frontend](https://github.com/takeover-c/flare-wtf-frontend) repository to see the working demo.

## License

[MIT](LICENSE)
