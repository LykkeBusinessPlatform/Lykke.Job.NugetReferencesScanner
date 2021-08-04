## Lykke.NuGetReferencesScanner.App

This console app scans BitBucket / Github repositories and compares all found packages to their current versions on nuget.org. The report is saved as report.csv in the app's working folder.

## Example configuration

```
{
    "Github": {
        "organization": "",
        "key": ""
  },
    "BitBucket": {
        "account": "",
        "key": "",
        "secret": ""
    },
    "mode": "all",
    "whitelist": ["repo1", "repo2"]
}
```

## Bitbucket

`account` - name of Bitbucket account that needs to be scanned. It must be a team account.

`key` - OAuth2 key

`secret` - OAuth2 secret

OAuth2 pair can be created at https://bitbucket.org/{{account}}/workspace/settings/api (_Workspace settings_ / _OAuth consumers_ / _Add consumer_). This must be a private consumer key to enable the `client_credentials` grant type.

## Github

`organization` - name of github account that needs to be scanned

`key` - personal access token, can be created [here](https://github.com/settings/tokens)

## Mode

Packages can be filtered by prefix. Currently the prefixes are hardcoded in the file _ProjectFileParser.cs_

| Mode    | Comment                                          |
| ------- | ------------------------------------------------ |
| all     | process all packages                             |
| include | only packages with a prefix will be processed    |
| exclude | only packages without a prefix will be processed |

If not set, it defaults to `all`.

## Whitelist

If set, the app will ignore all other repositories.

## Excel

Sometimes Excel would treat package's version number as date. In this case please use the Import Data Sources feature as described [here (Option 2)](https://www.winhelponline.com/blog/stop-excel-convert-text-to-number-date-format-csv-file/)
