# Bugtracker Console Usage

The Bugtracker console is available when no GUI plugin is loaded, or can be used for scripted/automated runs by passing a command directly as a CLI argument.

## Running modes

**Interactive mode** — launch the executable with no arguments to open a persistent prompt:
```
BugtrackerSystem.exe
bugtracker >
```

**Single-command mode** — pass a command as CLI arguments for scripted use:
```
BugtrackerSystem.exe capture -full
BugtrackerSystem.exe run MyApp
```

---

## Typical workflow

```
bugtracker > problem select "Application Crash"
bugtracker > run
```

Or the long-hand equivalent:
```
bugtracker > util init
bugtracker > capture -full
bugtracker > send default
```

---

## Command reference

Commands and their aliases are interchangeable: `capture` and `capt` do the same thing.
Subcommands can also be chained: `capture -log MyApp`.

---

### `help` · `hlp`
Shows a list of all top-level commands with their descriptions.

```
bugtracker > help
```

---

### `run`
**Captures a screenshot and all logs, then sends to targets — all in one step.**
Respects the currently selected problem category (see `problem select`). Falls back to default targets if no category is selected.

```
run [application] [application2 ...]
```

| Argument | Description |
|----------|-------------|
| *(none)* | Captures logs from all installed applications |
| `application ...` | Captures logs from the specified applications only |

**Examples:**
```
bugtracker > run
bugtracker > run MyApp ServiceApp
```

---

### `capture` · `capt`

Captures data from the host PC. Running `capture` alone creates a new bugtracker folder.

#### `capture -full` · `capture -f`
Captures a screenshot and log files.

```
capture -full [application] [application2 ...]
```

| Argument | Description |
|----------|-------------|
| *(none)* | Captures logs from all installed applications |
| `application ...` | Captures logs from the specified applications only |

```
bugtracker > capture -full
bugtracker > capture -full MyApp ServiceApp
```

#### `capture -log` · `capture -l`
Captures log files only (no screenshot).

```
capture -log <application> [application2 ...]
```

```
bugtracker > capture -log MyApp
bugtracker > capture -log MyApp ServiceApp RemoteTool
```

#### `capture -log all` · `capture -l -a`
Captures log files from all installed applications.

```
bugtracker > capture -log all
```

#### `capture -screen` · `capture -s`
Takes a screenshot only.

```
bugtracker > capture -screen
```

#### `capture -path` · `capture -p`
Shows the path of the current bugtracker capture folder.

```
bugtracker > capture -path
```

#### `capture -path open` · `capture -p o`
Opens the current capture folder in Explorer.

```
bugtracker > capture -path open
```

---

### `send` · `snd`

Sends the current capture to targets.

#### `send default` · `send dft`
Sends to all default targets configured in the main config.

```
bugtracker > send default
```

> **Note:** `send default` always uses the configured default targets regardless of any selected problem category. Use `run` instead for category-aware sending.

---

### `problem` · `prob`

Manage the active problem category. Selecting a category affects which targets `run` sends to (if the category has category-specific targets configured).

#### `problem list` · `problem ls`
Lists all configured problem categories and marks the currently selected one.

```
bugtracker > problem list
```

#### `problem select` · `problem sel`
Selects a problem category by name.

```
problem select <category-name>
```

Category names with spaces are supported — pass the full name:
```
bugtracker > problem select Application Crash
bugtracker > problem select General Error
```

#### `problem info` · `problem inf`
Shows the name, ticket abbreviation, description, and targets of the currently selected category.

```
bugtracker > problem info
```

#### `problem clear` · `problem clr`
Clears the active category selection, reverting `run` to use default targets.

```
bugtracker > problem clear
```

---

### `pcinfo` · `pcinf`

Shows host PC information: hostname, domain, user, IP, MAC, and last boot time.

```
bugtracker > pcinfo
```

#### `pcinfo variables` · `pcinfo vars`
Shows all Bugtracker variable substitution values — the variables available as `%varname%` placeholders in the config (e.g. `%clientname%`, `%date%`, `%hostname%`). Dynamic variables (recalculated on each use) are marked accordingly.

```
bugtracker > pcinfo variables
```

#### `pcinfo variables all` · `pcinfo vars all`
Shows all variables including all Windows environment variables (e.g. `%PATH%`, `%USERPROFILE%`).

```
bugtracker > pcinfo variables all
```

#### `pcinfo plugins` · `pcinfo plug`
Lists all currently loaded plugins with their version and author.

```
bugtracker > pcinfo plugins
```

---

### `applications` · `apps`

Inspect and manage the application list loaded from configuration.

#### `applications logs` · `applications -l`
Shows the configured log paths for every application.

```
bugtracker > applications logs
```

#### `applications logs installed` · `applications -l -i`
Same as above but only for applications whose executable is detected as installed on the current machine.

```
bugtracker > applications logs installed
```

---

### `application` · `app`

Inspect a single application.

#### `application log` · `application -l`
Shows the configured log paths for a specific application.

```
application log <application-name>
```

```
bugtracker > application log MyApp
```

---

### `logger` · `log`

Manage the internal Bugtracker logger.

#### `logger log` · `logger -l`
Writes a message to the Bugtracker log file at Info severity.

```
logger log <message words ...>
```

```
bugtracker > logger log Starting manual capture session
```

#### `logger enabled` · `logger -e`
Enables or disables the logger for the current session.

```
logger enabled <true|false>
```

```
bugtracker > logger enabled false
bugtracker > logger enabled true
```

#### `logger path` · `logger -p`
Shows the path of the current Bugtracker log file.

```
bugtracker > logger path
```

#### `logger status` · `logger -s`
Shows whether the logger is currently enabled.

```
bugtracker > logger status
```

---

### `target`

Inspect configured targets.

#### `target list`
Lists all targets from the running configuration with their type and settings.

```
bugtracker > target list
```

---

### `show` · `shw`

Inspect the running configuration.

#### `show config` · `show conf`
Dumps a summary of the running configuration: PC info, active folder, logger state, log severity, and target path.

```
bugtracker > show config
```

#### `show path`
Shows the full path of the current bugtracker capture folder. If no folder has been created yet, suggests running `util init`.

```
bugtracker > show path
```

---

### `util` · `utl`

Utility commands for folder and log management.

#### `util init`
Creates a new bugtracker capture folder and sets it as the active folder.

```
bugtracker > util init
```

#### `util delete` · `util del`
Deletes all log files for the specified application(s).

```
util delete <application> [application2 ...]
```

```
bugtracker > util delete MyApp
bugtracker > util delete MyApp ServiceApp
```

#### `util rename` · `util rnm`
Renames all log files for the specified application(s).

```
util rename <application> [application2 ...]
```

```
bugtracker > util rename MyApp
```

---

### `clear` · `clr`
Clears the console screen.

```
bugtracker > clear
```

---

### `exit`
Exits the interactive console.

```
bugtracker > exit
```

---

## Variable substitution

Many configuration values (paths, filenames, email subjects) support `%varname%` placeholders that are substituted at runtime. Use `pcinfo variables` to see all available values.

Common Bugtracker variables:

| Variable | Example value | Notes |
|----------|--------------|-------|
| `%hostname%` | `WORKSTATION-01` | Machine name |
| `%clientname%` | `WORKSTATION-01` | Same as hostname unless in a remote session |
| `%domainName%` | `CORP` | Windows domain |
| `%userName%` | `CORP\jsmith` | Currently logged-in user |
| `%ipAddress%` | `192.168.1.50` | IPv4 address |
| `%macAddress%` | `00C04FB3271C` | MAC address |
| `%date%` | `04-10-2026` | Current date (dynamic) |
| `%time%` | `14-32-05` | Current time (dynamic) |
| `%idString%` | `WORKSTATION-01` | Client identifier, includes remote host if in RDP session |
| `%abbrev%` | `app-crash` | Ticket abbreviation of selected problem category (dynamic) |

All Windows environment variables are also available: `%USERNAME%`, `%USERPROFILE%`, `%ProgramData%`, etc.
