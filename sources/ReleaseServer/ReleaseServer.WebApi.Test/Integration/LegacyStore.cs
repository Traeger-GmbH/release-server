//--------------------------------------------------------------------------------------------------
// <copyright file="LegacyStore.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using System.Text;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// Writes a data set into an artifact directory exactly as it can be found on existing installations.
    /// The set mirrors properties of the production data: meta files in camelCase (hand made uploads) and
    /// PascalCase (written by the server itself), upper case names, unusual platforms, pre-releases,
    /// release notes with broken encoding and an empty product directory.
    /// </summary>
    public static class LegacyStore
    {
        // Release notes as they are uploaded by hand (camelCase, date only).
        public const string ReleaseNotesFull = @"{
  ""releaseDate"": ""2021-05-07"",
  ""isPreviewRelease"": false,
  ""isSecurityPatch"": false,
  ""changes"": {
    ""de"": [
      { ""platforms"": [""linux/rpi""], ""added"": [], ""fixed"": [""Das Problem wurde behoben.""], ""breaking"": [], ""deprecated"": [] },
      { ""platforms"": null, ""added"": [""Feature xyz wurde hinzugefügt.""], ""fixed"": [], ""breaking"": [""Komponente D wurde ersetzt.""], ""deprecated"": [""Funktion foo entfällt.""] }
    ],
    ""en"": [
      { ""platforms"": [""linux/rpi""], ""added"": [], ""fixed"": [""The problem has been fixed.""], ""breaking"": [], ""deprecated"": [] },
      { ""platforms"": null, ""added"": [""Feature xyz has been added.""], ""fixed"": [], ""breaking"": [""Component D has been replaced.""], ""deprecated"": [""Function foo will be removed.""] }
    ]
  }
}";

        // Release notes with German changes only and missing optional properties.
        public const string ReleaseNotesGermanOnly = @"{
  ""releaseDate"": ""2019-02-21"",
  ""changes"": {
    ""de"": [ { ""added"": [""Version HW 3 Freigabe""], ""fixed"": [""Kein Neustart nach Firmwaretransfer.""] } ],
    ""en"": []
  }
}";

        // Release notes as written by the server itself (PascalCase, full date time, null lists).
        public const string ReleaseNotesPascalCase = @"{
  ""ReleaseDate"": ""2023-02-20T00:00:00"",
  ""IsPreviewRelease"": false,
  ""IsSecurityPatch"": true,
  ""Changes"": {
    ""de"": [
      {
        ""Platforms"": null,
        ""Added"": null,
        ""Fixed"": [
          ""Sicherheitslücke geschlossen.""
        ],
        ""Breaking"": null,
        ""Deprecated"": null
      }
    ],
    ""en"": [
      {
        ""Platforms"": null,
        ""Added"": null,
        ""Fixed"": [
          ""Security issue fixed.""
        ],
        ""Breaking"": null,
        ""Deprecated"": null
      }
    ]
  }
}";

        // Pre-release with a UTC time stamp.
        public const string ReleaseNotesPreview = @"{
  ""releaseDate"": ""2024-03-22T10:15:00Z"",
  ""isPreviewRelease"": true,
  ""changes"": {
    ""en"": [ { ""added"": [""Preview of feature abc.""] } ]
  }
}";

        public const string DeploymentCamelCase = @"{
  ""releaseNotesFileName"": ""releaseNotes.json"",
  ""artifactFileName"": ""{0}""
}";

        public const string DeploymentPascalCase = @"{
  ""ReleaseNotesFileName"": ""releaseNotes.json"",
  ""ArtifactFileName"": ""{0}""
}";

        /// <summary>
        /// Release notes containing a byte sequence that is not valid UTF-8 (a single Latin-1 'Ü'),
        /// like the broken umlauts found in production data.
        /// </summary>
        public static byte[] ReleaseNotesBrokenEncoding()
        {
            var head = Encoding.UTF8.GetBytes(@"{ ""releaseDate"": ""2014-12-12"", ""changes"": { ""de"": [ { ""fixed"": [""ISDN Anwahl, DF");
            var tail = Encoding.UTF8.GetBytes(@"-Benutzer wurden nicht mitgesendet.""] } ], ""en"": [] } }");
            return head.Concat(new byte[] { 0xDC }).Concat(tail).ToArray();
        }

        public static byte[] ArtifactContent(string name)
        {
            return Encoding.UTF8.GetBytes("artifact payload of " + name);
        }

        /// <summary>
        /// Creates the complete legacy data set in the given artifact directory.
        /// </summary>
        public static void Create(string artifactDirectory)
        {
            // Product with several platforms, versions and pre-releases (camelCase and PascalCase mixed).
            AddRelease(artifactDirectory, "alpha", "windows", "x86", "1.0", "alpha-setup.exe", ReleaseNotesFull, pascalCase: false);
            AddRelease(artifactDirectory, "alpha", "windows", "x64", "1.0", "alpha-setup.exe", ReleaseNotesFull, pascalCase: false);
            AddRelease(artifactDirectory, "alpha", "linux", "x64", "1.0", "alpha.tar.gz", ReleaseNotesFull, pascalCase: true);
            AddRelease(artifactDirectory, "alpha", "windows", "any", "1.9", "alpha-1.9.zip", ReleaseNotesGermanOnly, pascalCase: false);
            AddRelease(artifactDirectory, "alpha", "windows", "any", "1.10", "alpha-1.10.zip", ReleaseNotesPascalCase, pascalCase: true);
            AddRelease(artifactDirectory, "alpha", "windows", "any", "1.10-beta", "alpha-1.10-beta.zip", ReleaseNotesPreview, pascalCase: false);
            AddRelease(artifactDirectory, "alpha", "windows", "any", "1.10-rc", "alpha-1.10-rc.zip", ReleaseNotesPreview, pascalCase: false);

            // Upper case names, a hyphen in the operating system and a file name with spaces.
            AddRelease(artifactDirectory, "s5-lan-3964R-lan-manager", "Windows", "X86", "1.36", "Setup S5 LAN.exe", ReleaseNotesGermanOnly, pascalCase: false);
            AddRelease(artifactDirectory, "s5-lan-3964R-lan-manager", "windows-xp", "any", "1.7", "s5lan-1.7.zip", ReleaseNotesBrokenEncoding(), pascalCase: true);

            // Device firmware with a four part version and a numeric architecture.
            AddRelease(artifactDirectory, "teleprof2-os-firmware", "teleprof", "2", "8.14.0.1", "firmware.bin", ReleaseNotesFull, pascalCase: false);

            // Product directory without any release (exists in production).
            Directory.CreateDirectory(Path.Combine(artifactDirectory, "pc-s7-link-demo2"));
        }

        public static void AddRelease(string artifactDirectory, string product, string os, string architecture,
            string version, string artifactFileName, string releaseNotes, bool pascalCase)
        {
            AddRelease(artifactDirectory, product, os, architecture, version, artifactFileName,
                Encoding.UTF8.GetBytes(releaseNotes), pascalCase);
        }

        public static void AddRelease(string artifactDirectory, string product, string os, string architecture,
            string version, string artifactFileName, byte[] releaseNotes, bool pascalCase)
        {
            var directory = Path.Combine(artifactDirectory, product, os, architecture, version);
            Directory.CreateDirectory(directory);

            var deployment = (pascalCase ? DeploymentPascalCase : DeploymentCamelCase).Replace("{0}", artifactFileName);
            File.WriteAllBytes(Path.Combine(directory, "deployment.json"), Encoding.UTF8.GetBytes(deployment));
            File.WriteAllBytes(Path.Combine(directory, "releaseNotes.json"), releaseNotes);
            File.WriteAllBytes(Path.Combine(directory, artifactFileName), ArtifactContent($"{product}/{os}/{architecture}/{version}"));
        }
    }
}
