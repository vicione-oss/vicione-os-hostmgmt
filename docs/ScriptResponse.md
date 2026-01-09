# ScriptResponse

Die `ScriptResponse` besteht aus einem `ExitCode`, dem `StandardOutput` sowie dem `StandardError`.

Folgende Antworten sind beim Abfragen (GET) bzw. Setzen (SET) von Einstellungen möglich:

| Aktion | ExitCode | StandardOutput           | StandardError      |
| ------ | :------: | ------------------------ | ------------------ |
| GET    |    0     | `JSON` (Settings-Objekt) | -                  |
| &nbsp; |    0     | `JSON` (Settings-Objekt) | `Text` (Warnungen) |
| &nbsp; |   > 0    | -                        | `Text` (Fehler)    |
| SET    |    0     | `Text` (Informationen)   | -                  |
| &nbsp; |    0     | `Text` (Informationen)   | `Text` (Warnungen) |
| &nbsp; |   > 0    | -                        | `Text` (Fehler)    |
