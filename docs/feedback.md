# Feedback log

Comments from the TekkenMods page (https://tekkenmods.com/mod/7283/tekken-8-mod-manager) that need work or a reply. Once the repo is on GitHub each actionable row becomes an issue titled with the comment id, for example `[tekkenmods #38340] Start TEKKEN 8 reports "executable not found"`, and the fix commit says `Fixes #n`.

The loop: read the new comments, add a row, fix, release, reply with the version and ask for a short yes or no. Rows are never deleted; a closed row keeps its release.

Last read: 2026-10-08.

| Comment | Kind | What | Status |
|:---|:---|:---|:---|
| #38340 | bug | "Start TEKKEN 8" says the executable is not found when the game location is the Paks folder | fixed in `1076fc7` (the game folder was computed one level too high since 1.1.0); released in 1.1.1; reply to the reporter naming the version |
| #38159 | bug? | Disabling a mod in the manager leaves it active in the game | open: cause unknown, needs the reporter's mod layout |
| #37873 | feature | Priority between overlapping mods | open, not started |
| #38843 | feature | Support a mod that replaces loose files under `Polaris\Content\Movies\usm\StageSelect`, outside `Paks` | open, not started |
| #38317 | question | Is this the most functional mod manager? | reply or none |
| #37952 | feedback | Promised to test 1.1.0-experimental and report back | waiting on the reporter |
| #37843 | question | Windows warns about the unsigned exe | answered; candidate FAQ line |
