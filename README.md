# Settings
Holds global configuration. Available options:
## Global settings
| Option | Required | Comment |
|--------|----------|---------|
| LogLevel | yes  | [LogLevels](https://github.com/Shikareth/LogTriggers/blob/main/ConsoleApplication/Enums/LogLevel.cs) |
| TimestampFormat | yes  | [DateTime Format](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings) |
| TimestampFilter | yes  | Regex to extract sub-string from line. Must contain **ONLY** one capture group (in braces) |
| UpdateDelay | yes  | Delay after last read in milliseconds |
| Browser | no | Do not use |
| BrowserEntrySite | no | Do not use |
| BrowserPosition | no | look [LAWindowPosition](https://github.com/Shikareth/LogTriggers/blob/main/ConsoleApplication/Settings/LAWindowPosition.cs) |
| LogFolder | no | Main path to files (overridden by file config) |
| LogFiles| yes | List of files to monitor |

## File settings
| Option | Required | Comment |
|--------|----------|---------|
| Label | Required | File custom identifier to avoid printing/checking filename |
| Filename | Required | filename.extension |
| TimestampFormat | Required | [DateTime Format](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings) |
| TimestampFilter | Required | Regex to extract sub-string from line. Must contain **ONLY** one capture group (in braces) |

## Example

    "Settings": {
      "LogLevel": "INFO",
      "LogFolder": "D:\\#_PROJECTS\\2024_01-BYK_Chemie\\documents\\ISSUES\\WMS_doubleLUStock\\Logs_to_be_checked\\20250413",
      "TimestampFormat": "yyyy-MM-dd HH:mm:ss.FFFF",
      "TimestampFilter": "(\\d+-\\d+-\\d+\\s\\d+:\\d+:\\d+\\.\\d+)",
      "UpdateDelay": 500,
      "LogFiles": [
        {
          "Label": "WMS",
          "Filename": "MWMS.20250413.0.log"
        },
        {
          "Label": "MFC",
          "Filename": "MFlow.20250413.0.log"
        },
        {
          "Label": "SAP",
          "Filename": "SAPConnector.20250413.0.log",
          "TimestampFormat": "yyyy-MM-dd HH:mm:ss.FFFF",
          "TimestampFilter": "(\\d+-\\d+-\\d+\\s\\d+:\\d+:\\d+\\.\\d+)"
        }
      ]
    }
# Runtime
## Console arguments
> TODO

## Starting
run **.exe**. Requires existence of valid **appsettings.json**

## Closing

    ctrl+c

