# Bugtracker Configuration Reference

This document describes all available options for both configuration files used by Bugtracker V2.

---

## Files

| File | Purpose |
|---|---|
| `bugtracker_config_startup.xml` | Startup behavior, server location, logging settings |
| `bugtracker_config_main.xml` | Targets, problem categories, and application definitions |

Configuration files are loaded with the following priority:
1. Web backend (`configWebserverUrl` in startup config)
2. SMB share (`loadConfigsFrom` in startup config)
3. Local fallback (`configs\` folder in the install directory — populated automatically when tier 1 or 2 succeeds; also contains the defaults shipped with the installer)

---

## Variable Substitution

String values in both config files support `%variablename%` substitution. All Windows environment variables are available in addition to the built-in Bugtracker variables below.

### Built-in Bugtracker Variables

| Variable | Description |
|---|---|
| `%date%` | Current date (dynamic, refreshed at use) |
| `%time%` | Current time (dynamic, refreshed at use) |
| `%idString%` | Unique identification string for the current session |
| `%abbrev%` | Ticket abbreviation of the selected problem category (dynamic) |
| `%hostname%` | PC hostname |
| `%clientname%` | RDP client hostname; falls back to `%hostname%` when not in a remote session |
| `%domainName%` | Active Directory domain name |
| `%ipAddress%` | IP address of the machine |
| `%macAddress%` | MAC address of the machine |
| `%userName%` | Currently logged-in Windows username |
| `%version%` | Bugtracker application version |

### Common Windows Environment Variables

| Variable | Example Value |
|---|---|
| `%USERPROFILE%` | `C:\Users\username` |
| `%COMPUTERNAME%` | `WORKSTATION01` |
| `%USERDOMAIN%` | `CORP` |
| `%ProgramData%` | `C:\ProgramData` |
| `%LOCALAPPDATA%` | `C:\Users\username\AppData\Local` |
| `%SESSIONNAME%` | `Console` or `RDP-Tcp#0` |

---

## Startup Configuration (`bugtracker_config_startup.xml`)

Root element: `<configuration>`  
Contains a single `<startup>` element.

### `<startup>` Attributes

| Attribute | Type | Required | Default | Description |
|---|---|---|---|---|
| `startGUI` | bool | yes | `true` | Launch the GUI plugin on startup. Set to `false` for console-only mode. |
| `firstStartup` | bool | yes | `false` | Marks this as a first run. Triggers first-startup behavior. Automatically set to `false` after first run. |
| `mainserver` | string | yes | — | IP address or hostname of the main server. Used for connectivity checks. |
| `configWebserverUrl` | string | no | — | HTTP/HTTPS URL of the Django web server used to download configuration files. |
| `loadConfigsFrom` | string | yes | — | UNC path or local directory from which `bugtracker_config_main.xml` (and other config files) are loaded. |
| `loggingSeverity` | int | no | `3` | Log verbosity level. `1` = Error only, `2` = Warning+, `3` = Info+, `4` = Debug (all). |
| `fileCheckTimeoutMs` | int | no | `1000` | Timeout in milliseconds for checking whether network directories exist. Prevents hangs on unreachable shares. |

### Example

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <startup
    startGUI="true"
    firstStartup="false"
    mainserver="10.74.10.100"
    configWebserverUrl="http://bugtracker.radiologie.intern"
    loadConfigsFrom="\\10.74.10.100\bugTracker\bugtrackerV2Current"
    loggingSeverity="3"
    fileCheckTimeoutMs="1000">
  </startup>
</configuration>
```

---

## Main Configuration (`bugtracker_config_main.xml`)

Root element: `<configuration>`  
Contains three top-level sections: `<targets>`, `<problem-categories>`, and `<applications>`.

> **Important:** `<targets>` must appear before `<problem-categories>` because problem categories reference targets by name.

---

## `<targets>` Section

Contains one or more `<target>` elements. Each target defines a destination where bugtracker data can be sent.

### Common Target Attributes

| Attribute | Type | Required | Default | Description |
|---|---|---|---|---|
| `type` | string | yes | — | Target type: `folder`, `mail`, `powershell`, or `webupload`. |
| `name` | string | yes | — | Unique display name. Referenced by problem categories. |
| `default` | bool | no | `false` | If `true`, this target is pre-selected in the GUI send dialog. |
| `obligatory` | bool | no | `false` | If `true`, data is always sent to this target regardless of user selection. |

---

### Target Type: `folder`

Copies the bugtracker folder to a local or network path.

| Attribute | Type | Required | Description |
|---|---|---|---|
| `path` | string | yes | Destination directory. Supports UNC paths and environment variables. |
| `foldername` | string | no | Template for the folder name created at the destination. Supports variables. If omitted, the original bugtracker folder name is used. |
| `address` | string | no | Display label for the target (UI only, no functional effect). |

**Example:**
```xml
<target type="folder" name="network-folder" default="false" obligatory="false"
    path="\\10.74.10.100\bugTracker"
    foldername="Bugtracker-%idString%.%USERDOMAIN%-%date%-%time%"/>
```

---

### Target Type: `mail`

Sends the bugtracker folder as a ZIP attachment via SMTP email.

| Attribute | Type | Required | Description |
|---|---|---|---|
| `sender` | string | yes | From address. |
| `recipient` | string | yes | To address. |
| `smtpserver` | string | yes | SMTP server hostname or IP. |
| `smtpport` | int | yes | SMTP port (e.g., `587` for STARTTLS, `465` for SSL). |
| `smtpssl` | bool | yes | Enable SSL/TLS for the SMTP connection. |
| `smtpuser` | string | yes | SMTP authentication username. |
| `smtppass` | string | yes | SMTP authentication password. |
| `subject` | string | yes | Email subject. Supports variable substitution. |
| `htmltemplate` | string | yes | HTML body source. Accepts a file path, a URL, or an inline HTML string — see below. |
| `attachzip` | bool | yes | If `true`, attaches a ZIP of the bugtracker folder to the email. |

#### `htmltemplate` modes

The value is auto-detected at send time — no extra attribute is needed:

| Detected as | Condition | Behaviour |
|---|---|---|
| **File path** | Does not start with `http://` / `https://` and does not start with `<` | Read from disk. Works for local paths and UNC shares (e.g. `\\server\share\template.html`). |
| **URL** | Starts with `http://` or `https://` | Downloaded via HTTP GET at send time (15 s timeout). Compatible with web-only deployments where no SMB share is available. |
| **Inline HTML** | Trimmed value starts with `<` | Used directly as the email body. Useful for simple templates that need no external file. |

Variable substitution (`%hostname%`, `{screenshots}`, etc.) is applied to the resolved HTML regardless of which mode is used.

**Example — file path (local or UNC share):**
```xml
<target type="mail" name="mail-support" default="false"
    sender="support@example.com"
    recipient="support@example.com"
    smtpserver="smtp.example.com"
    smtpport="587"
    smtpssl="True"
    smtpuser="support@example.com"
    smtppass="secret"
    subject="Bugtracker: %clientname%.%USERDOMAIN% (%date%-%time%)"
    htmltemplate="\\server\share\mailtemplate.html"
    attachzip="true"/>
```

**Example — URL (web-only deployment, no SMB share required):**
```xml
    htmltemplate="https://your-bugtracker-server/static/mailtemplate.html"
```

**Example — inline HTML (no external file required):**
```xml
    htmltemplate="&lt;html&gt;&lt;body&gt;&lt;p&gt;A bugtrack was submitted from &lt;strong&gt;%hostname%&lt;/strong&gt;.&lt;/p&gt;&lt;p&gt;{screenshots}&lt;/p&gt;&lt;/body&gt;&lt;/html&gt;"
```

---

### Target Type: `powershell`

Executes a PowerShell script after data is collected, passing bugtracker information as parameters.

| Attribute | Type | Required | Default | Description |
|---|---|---|---|---|
| `path` | string | yes | — | Local or UNC path to the `.ps1` script file. Used as fallback if `downloadLink` fails. |
| `downloadLink` | string | no | — | HTTP/HTTPS URL to download the script from before execution. |
| `saveAs` | string | no | `[InstallDir]\scripts\[filename]` | Local path where the downloaded script is saved. |
| `passvariables` | bool | no | `false` | Pass the Bugtracker variable dictionary as a hashtable to the script. |
| `passfolders` | bool | no | `false` | Pass the list of captured bugtracker folder paths to the script. |
| `passproblemcat` | bool | no | `false` | Pass the selected problem category name to the script. |
| `logdefault` | bool | no | `true` | Forward the script's default output stream to the Bugtracker log. |
| `logerrors` | bool | no | `true` | Forward the script's error stream to the Bugtracker log. |
| `logwarnings` | bool | no | `true` | Forward the script's warning stream to the Bugtracker log. |
| `loginformations` | bool | no | `true` | Forward the script's information stream to the Bugtracker log. |
| `logprogress` | bool | no | `false` | Forward the script's progress stream to the Bugtracker log. |

**Example:**
```xml
<target type="powershell" name="custom-script" default="false"
    path="\\server\share\myscript.ps1"
    passfolders="true"
    passproblemcat="true"
    logdefault="true"
    logerrors="true"/>
```

---

### Target Type: `webupload`

Uploads bugtracker folders to a Bugtracker web backend via HTTP REST API.

| Attribute | Type | Required | Default | Description |
|---|---|---|---|---|
| `serverurl` | string | yes | — | Base URL of the Bugtracker Django web server (e.g., `http://bugtracker.example.intern`). |
| `verbose` | bool | no | `false` | Enable verbose logging from the upload library. |

**Example:**
```xml
<target type="webupload" name="Web-Upload" default="true" obligatory="true"
    serverurl="http://bugtracker.example.intern"/>
```

---

## `<problem-categories>` Section

Defines the problem types shown to the user. Each category controls which applications are captured and which targets receive the data.

### `<problem-category>` Attributes

| Attribute | Type | Required | Description |
|---|---|---|---|
| `name` | string | yes | Display name shown in the GUI dropdown. |
| `ticket` | string | yes | Short abbreviation used as ticket prefix (`%abbrev%` variable) and for the web upload ticket ID. |

### Child Elements

#### `<description>`

| Attribute | Type | Required | Description |
|---|---|---|---|
| `text` | string | yes | Pre-filled problem description text shown to the user. |

Multiple `<description>` elements are allowed and will be concatenated.

#### `<app-selection>`

Text content (not an attribute). Comma-separated list of application names to pre-select for capture. Special values:

| Value | Effect |
|---|---|
| `All` | Select all configured applications. |
| `Screen` | Capture screenshots from all monitors. |
| `AppName` | Select the application with matching `name` attribute. |

Multiple values can be combined: `Screen,IP8,XR5`

#### `<target>`

| Attribute | Type | Required | Description |
|---|---|---|---|
| `name` | string | yes | Name of a target defined in `<targets>`. The data will be sent to this target when this category is selected. |

Multiple `<target>` elements are allowed per category.

### Example

```xml
<problem-category name="XR - Fehler" ticket="xr5-error">
  <description text="Ist langsam oder reagiert nicht:"/>
  <app-selection>XR5,XR-Wordplugin,Screen</app-selection>
  <target name="Web-Upload"/>
</problem-category>
```

---

## `<applications>` Section

Defines the applications Bugtracker can collect logs from.

### `<application>` Attributes

| Attribute | Type | Required | Description |
|---|---|---|---|
| `name` | string | yes | Unique application name. Referenced in `<app-selection>`. |
| `executable` | string | yes | Path to the application's executable or install directory. Used for the `installed` show check. |
| `standard` | bool | yes | If `true`, this application is pre-selected when no problem category is active. |
| `show` | string | yes | Visibility in the GUI: `always` (always shown), `installed` (only if `executable` path exists), `never` (hidden, still accessible programmatically). |

### Child Elements of `<application>`

#### `<log>`

Defines a log file or set of log files to collect.

| Attribute | Type | Required | Description |
|---|---|---|---|
| `location` | string | yes | Where to look: `client` (local machine or via `\\tsclient\` for RDP), `server` (network share), `host` (RDP host machine). Case-insensitive. |
| `path` | string | yes | Directory path. Supports environment variables and `%clientname%`. |
| `filename` | string | yes | Filename or glob pattern (e.g., `*.log`, `app_*.log`). Supports variables. |
| `find` | string | yes | Find strategy: `NEW` (most recently modified file), `ALL` (every matching file), `AGE` (files modified within a time window). |
| `minage` | int | no | Minimum age in minutes (only used with `find="AGE"`). Default: `0`. |
| `maxage` | int | no | Maximum age in minutes (only used with `find="AGE"`). Default: `60`. |
| `lastlines` | int | no | Trim copied log to the last N lines. If omitted, the full file is copied. |

#### `<pre-fetch>`

A shell command or script executed before log files are collected from this application.

| Attribute | Type | Required | Description |
|---|---|---|---|
| `path` | string | yes | Command or script path to execute. |

#### `<post-fetch>`

A shell command or script executed after log files are collected from this application.

| Attribute | Type | Required | Description |
|---|---|---|---|
| `path` | string | yes | Command or script path to execute. |

#### `<powershell>`

A PowerShell script executed as part of the capture for this application (pre or post).

| Attribute | Type | Required | Default | Description |
|---|---|---|---|---|
| `execution` | string | yes | — | When to run: `pre-fetch` (before log collection) or `post-fetch` (after). |
| `path` | string | yes | — | Path to the `.ps1` script. Fallback if `downloadLink` fails. |
| `downloadLink` | string | no | — | URL to download the script from. |
| `saveAs` | string | no | — | Local save path for the downloaded script. |
| `passfolders` | bool | no | `false` | Pass bugtracker folder paths to the script. |
| `passvariables` | bool | no | `false` | Pass the variable dictionary to the script. |
| `passproblemcat` | bool | no | `false` | Pass the selected problem category to the script. |
| `logdefault` | bool | no | `true` | Forward default output to Bugtracker log. |
| `logerrors` | bool | no | `true` | Forward error stream to Bugtracker log. |
| `logwarnings` | bool | no | `true` | Forward warning stream to Bugtracker log. |
| `loginformations` | bool | no | `true` | Forward information stream to Bugtracker log. |
| `logprogress` | bool | no | `false` | Forward progress stream to Bugtracker log. |

### Example

```xml
<application name="IP8" executable="C:\Program Files\DATA\xr pacs\ImagePro 8\" standard="true" show="installed">
  <log location="client" path="C:\ProgramData\DATA\ImagePro8Logs\" filename="ImagePro8_*.log" find="NEW"/>
  <log location="client" path="\\tsclient\c\ProgramData\DATA\ImagePro8Logs\" filename="ImagePro8_*.log" find="NEW"/>
  <log location="server" path="\\server\imagecall\" filename="*%clientname%*" find="NEW"/>
  <powershell execution="pre-fetch"
      downloadLink="https://bugtracker.example.intern/config/scripts/prep.ps1"
      passfolders="true"
      logdefault="true"/>
</application>
```
