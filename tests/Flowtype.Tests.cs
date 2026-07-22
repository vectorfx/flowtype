using System;
using System.Collections.Generic;
using Flowtype;

namespace Flowtype.Tests
{
    public sealed class TestRunner
    {
        public int RunAll()
        {
            int failures = 0;
            failures += AssertEqual("self correction", "I want pizza no I want pasta.", TextProcessor.Clean("I want pizza no I want pasta.", AppSettings.Defaults()));
            failures += AssertEqual("repeated words", "The cat.", TextProcessor.Clean("the the cat", AppSettings.Defaults()));
            failures += AssertEqual("fuzzy dictionary", "Open Settings.", FuzzySettingsTest());
            failures += AssertEqual("whisper repeat", "The team shipped the feature yesterday.",
                TextProcessor.Clean("The team shipped the feature yesterday. The team shipped the feature yesterday. The team shipped the feature yesterday.", AppSettings.Defaults()));
            failures += AssertTrue(TranscriptionQuality.ShouldReject("T", 500, 12000));
            failures += AssertFalse(TranscriptionQuality.ShouldReject("no", 500, 12000));
            failures += AssertContains("1.", TextProcessor.Clean("first get milk second get bread third get eggs", AppSettings.Defaults()));
            failures += AssertContains("- ", TextProcessor.Clean("bullet point apples bullet point bananas bullet point cherries", AppSettings.Defaults()));
            failures += AssertEqual("spoken period", "Hello.", TextProcessor.Clean("hello period", AppSettings.Defaults()));
            failures += AssertEqual("spoken question", "Ready?", TextProcessor.Clean("ready question mark", AppSettings.Defaults()));
            failures += AssertTrue(Hotkeys.IsChord("Win + Ctrl"));
            failures += AssertFalse(Hotkeys.IsChord("Right Ctrl"));
            failures += AssertEqual("default engine", "Local", AppSettings.Defaults().Engine);
            failures += AssertEqual("default cleanup", "BuiltIn", AppSettings.Defaults().CleanupProvider);
            AppSettings repaired = AppSettings.Defaults();
            repaired.CleanupProvider = "OpenRouter";
            repaired.OpenRouterModel = "openrouter/free";
            repaired.Repair();
            failures += AssertEqual("openrouter free blocked", "BuiltIn", repaired.CleanupProvider);
            return failures;
        }

        private static string FuzzySettingsTest()
        {
            AppSettings settings = AppSettings.Defaults();
            ForegroundInfo context = new ForegroundInfo();
            context.Title = "Cursor Settings";
            return TextProcessor.Clean("open setsings", settings, context);
        }

        private static int AssertEqual(string name, string expected, string actual)
        {
            if (String.Equals(expected, actual, StringComparison.Ordinal))
            {
                Console.WriteLine("PASS " + name);
                return 0;
            }
            Console.WriteLine("FAIL " + name + " expected=[" + expected + "] actual=[" + actual + "]");
            return 1;
        }

        private static int AssertContains(string needle, string haystack)
        {
            if (haystack != null && haystack.IndexOf(needle, StringComparison.Ordinal) >= 0)
            {
                Console.WriteLine("PASS contains " + needle);
                return 0;
            }
            Console.WriteLine("FAIL missing " + needle + " in [" + haystack + "]");
            return 1;
        }

        private static int AssertTrue(bool value)
        {
            if (value) { Console.WriteLine("PASS true"); return 0; }
            Console.WriteLine("FAIL expected true");
            return 1;
        }

        private static int AssertFalse(bool value)
        {
            if (!value) { Console.WriteLine("PASS false"); return 0; }
            Console.WriteLine("FAIL expected false");
            return 1;
        }
    }
}
