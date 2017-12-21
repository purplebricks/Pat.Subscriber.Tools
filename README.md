# Pat.Lite.Tools

This tool creates the subscriptions and topics required for a PatLite subscriber. The rules on the subscription are created by the subscriber at runtime.

## Installation

To install the dotnet tooling into a project add the following to a projects `.csproj` file:

```
<ItemGroup>
  <DotNetCliToolReference Include="PB.ITOps.Messaging.PatLite.Tools" Version="*" />
</ItemGroup>
```

## Usage

Navigate your terminal to the folder with the project you have installed the patlite tooling. Then run `donet restore`. At this point you are able to run `dotnet pat`. The pat tooling has a built in help which should help you run the tool.

## Authentication
This tool requires authentication into your azure subscription. To make this flow more straightforward your authentication tokens are encrypted and stored in the file `%APPDATA%\PatLite\Tokencache.dat`. If you do not wish for your credentials to be stored you can either delete the file once the tool has run or you can run `dotnet pat logout`