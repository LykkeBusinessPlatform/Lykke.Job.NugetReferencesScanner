## Lykke.NuGetReferencesScanner.App

This console app scans BitBucket / Github repositories and compares all found packages to their current versions on nuget.org. The report is saved as report.csv in the app's working folder.

## Example configuration

```
{
    "BitBucket": {
        "account": "",
        "key": "",
        "secret": ""
    },
    "mode": "all",
    "whitelist": ["repo1", "repo2"]
}
```

BitBucket account must be a team account.

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
