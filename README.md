# Jira Time Log Submitter - Blazor Server Side
Blazor Server-Side Project to submit time logs to Jira. Would be better as a Blazor Client Side PWA app, but debugging is currently broken (despite what the docs say).

*A lot of this is a work in progress, feel free to edit as you see fit*

This app was made with the sole intention of giving me a reason to mess wtih Blazor.  
It's a personal project and may not be updated for a long time.  
The goal is too allow me to submit my time logs to Jira from a single location quickly.

To run this project yourself, download/clone the code and setup `appsettings.local.json` with the values for `JiraSettings` following the pattern found in `appsettings.json`

Run the project, enter your Ticket numbers and time spent. Comments are optional.  
Generates text to be pasted into MS Teams for daily stand-ups
