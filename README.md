[![progress-banner](https://backend.codecrafters.io/progress/grep/229836a5-9dc6-411b-af32-8f868bb23d13)](https://app.codecrafters.io/users/codecrafters-bot?r=2qF)

This is a starting point for C# solutions to the
["Build Your Own grep" Challenge](https://app.codecrafters.io/courses/grep/overview).

[Regular expressions](https://en.wikipedia.org/wiki/Regular_expression)
(Regexes, for short) are patterns used to match character combinations in
strings. [`grep`](https://en.wikipedia.org/wiki/Grep) is a CLI tool for
searching using Regexes.

In this challenge you'll build your own implementation of `grep`. Along the way
we'll learn about Regex syntax, how parsers/lexers work, and how regular
expressions are evaluated.

**Note**: If you're viewing this repo on GitHub, head over to
[codecrafters.io](https://codecrafters.io) to try the challenge.

# Passing the first stage

The entry point for your `grep` implementation is in `src/Program.cs`. Study and
uncomment the relevant code, and push your changes to pass the first stage:

```sh
git commit -am "pass 1st stage" # any msg
git push origin master
```

Time to move on to the next stage!

# Stage 2 & beyond

Note: This section is for stages 2 and beyond.

1. Ensure you have `dotnet (9.0)` installed locally
1. Run `./your_program.sh` to run your program, which is implemented in
   `src/Program.cs`.
1. Commit your changes and run `git push origin master` to submit your solution
   to CodeCrafters. Test output will be streamed to your terminal.
   Blazor WebAssembly is actually a perfect fit for this — your C# code runs directly in the browser, no backend needed, and you can deploy the static output to anywhere including Vercel or GitHub Pages.

Why it works well here

Your existing classes (DigitToken, MatchHere, etc.) port over with zero changes
Blazor WASM compiles to WebAssembly and runs client-side
The UI is just a couple of input fields and a result — very simple Blazor component
You stay entirely in C# which you're already thinking in


The basic structure
One Blazor component with:
<input> for pattern
<input> for input string
<button> to test
<div> showing result
And your MatchPattern function gets called directly from the component's code-behind. That's it.


One thing to think about
Your current code uses Console.WriteLine for debugging — strip those out before wiring it into Blazor. Other than that your core logic should work as-is.

Honest assessment
This is probably a half day of work if you've never touched Blazor before. The learning curve is low if you know C# — it's mostly just Razor syntax which is similar to what you'd find in any MVC project.

Minimal viable UI
Honestly all you need is:

A heading explaining what it is
Pattern input field
Test input field
A button
A result that shows green/red or "match"/"no match"

That's five elements. You can make that look clean with just basic CSS or even Tailwind which has a Blazor integration.

Getting started path

dotnet new blazorwasm -o RegexEngine
Delete the boilerplate sample pages
Copy your token classes and match logic in as-is
Build one component with the UI above
Wire the button to call MatchPattern
Deploy the published output

The Blazor docs are good and the "publish to static files" story is straightforward for Vercel.

One suggestion
When you build it, add a few preset example patterns users can click to load — things like \d+, ca+t, [^abc]. It makes the demo self-explanatory without any instructions and shows off the range of what you built.
Want to tackle that after you finish the grep stages?
