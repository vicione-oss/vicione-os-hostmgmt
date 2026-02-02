# ScriptResponse

The `ScriptResponse` consists of the `ExitCode`, the `StandardOutput` and the `StandardError`.

The following responses are possible:

## Getting Settings or Data

| ExitCode | StandardOutput                                     | StandardError     |
| :------: | -------------------------------------------------- | ----------------- |
|    0     | `JSON` (Settings or parts of it)</br>`Text` (Data) | -                 |
|    0     | `JSON` (Settings or parts of it)</br>`Text` (Data) | `Text` (Warnings) |
|   > 0    | -                                                  | `Text` (Errors)   |

## Setting Settings or perform Action

| ExitCode | StandardOutput                | StandardError     |
| :------: | ----------------------------- | ----------------- |
|    0     | `Text` (optional Information) | -                 |
|    0     | `Text` (optional Information) | `Text` (Warnings) |
|   > 0    | -                             | `Text` (Errors)   |
