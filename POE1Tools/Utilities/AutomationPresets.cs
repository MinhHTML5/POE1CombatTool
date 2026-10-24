using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace POE1Tools.Utilities
{
    public enum MacroActionType
    {
        KeyDown,
        KeyUp,
        MouseDown,
        MouseUp
    }

    public class MacroAction
    {
        public MacroActionType Type { get; set; }
        // Milliseconds since the start of the recording
        public int Time { get; set; }
        // Used by KeyDown / KeyUp
        public Keys Key { get; set; }
        // Used by MouseDown / MouseUp. The position is in percent of the screen size (0 - 100),
        // so a macro still clicks at the right place on another resolution of the same ratio
        public MouseButtons Button { get; set; }
        public double XPercent { get; set; }
        public double YPercent { get; set; }

        // Position in pixels, only found in files saved by older versions. Converted by MacroPresetStore.Load
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? X { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Y { get; set; }

        [JsonIgnore]
        public bool IsMouse => Type == MacroActionType.MouseDown || Type == MacroActionType.MouseUp;

        public MacroAction Clone()
        {
            return (MacroAction)MemberwiseClone();
        }

        public string Describe()
        {
            switch (Type)
            {
                case MacroActionType.KeyDown: return "Key down";
                case MacroActionType.KeyUp: return "Key up";
                case MacroActionType.MouseDown: return "Mouse down";
                default: return "Mouse up";
            }
        }

        public string DescribeDetail()
        {
            return IsMouse ? Button + " at (" + XPercent.ToString("0.00") + "%, " + YPercent.ToString("0.00") + "%)" : Key.ToString();
        }
    }

    public enum MacroType
    {
        // Plays the recorded clicks and key presses
        Repeat,
        // Does nothing for some time, then ends
        Delay,
        // Clicks the item labels found on the screen, ends when there is none left
        ItemPickup
    }

    // The colors of an item label worth clicking, as the loot filter shows it
    public class LabelColor
    {
        // "#RRGGBB"
        public string Background { get; set; } = "#FFFFFF";
        public string Text { get; set; } = "#000000";

        public LabelColor()
        {
        }

        public LabelColor(System.Drawing.Color background, System.Drawing.Color text)
        {
            Background = ToHex(background);
            Text = ToHex(text);
        }

        [JsonIgnore]
        public System.Drawing.Color BackgroundColor => FromHex(Background);
        [JsonIgnore]
        public System.Drawing.Color TextColor => FromHex(Text);

        public static string ToHex(System.Drawing.Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        // Black if the text is not a valid color
        public static System.Drawing.Color FromHex(string hex)
        {
            if (hex != null && hex.Length == 7 && hex[0] == '#'
                && int.TryParse(hex.Substring(1), System.Globalization.NumberStyles.HexNumber, null, out int rgb))
            {
                return System.Drawing.Color.FromArgb(255, System.Drawing.Color.FromArgb(rgb));
            }
            return System.Drawing.Color.Black;
        }
    }

    // Used by MacroType.ItemPickup
    public class ItemPickupSettings
    {
        public List<LabelColor> Labels { get; set; } = new List<LabelColor>();
        // How far each of R, G, B can be from the background color. The text is anti-aliased, it gets twice that
        public int ColorTolerance { get; set; } = 25;
        // Wait this long after a click before looking again, the character has to walk to the item
        public int DelayAfterClick { get; set; } = 600;
        // Wait this long before looking again when nothing was found
        public int ScanInterval { get; set; } = 200;
        // The macro ends when nothing was found this many times in a row
        public int EmptyScansToEnd { get; set; } = 3;
        // The macro also ends after this many clicks, in case an item can't be picked up (inventory full...)
        public int MaxClicks { get; set; } = 30;
        // Where to look, in percent of the screen size. The default leaves the bottom HUD out
        public double RegionLeft { get; set; } = 0;
        public double RegionTop { get; set; } = 3;
        public double RegionRight { get; set; } = 100;
        public double RegionBottom { get; set; } = 82;

        public static ItemPickupSettings CreateDefault()
        {
            ItemPickupSettings settings = new ItemPickupSettings();
            // White box with red text, salmon box with black text, and the orange box of the uniques
            settings.Labels.Add(new LabelColor { Background = "#F0EDED", Text = "#C80F0F" });
            settings.Labels.Add(new LabelColor { Background = "#DF5F51", Text = "#190000" });
            settings.Labels.Add(new LabelColor { Background = "#B05D2A", Text = "#EBEBEB" });
            return settings;
        }
    }

    public enum ActionNodeKind
    {
        // Where the action begins. Its only link goes to the first macro, without any condition
        Start,
        // A link going here ends the action
        End,
        // A macro: something to play, a delay, or an item pickup
        Macro
    }

    // A node on the canvas of an action preset. A Macro node owns its macro: the type, and the settings of that type
    public class ActionNode
    {
        // Shown in the type dropdown of the Automation window, in the order of MacroType
        public static readonly string[] TypeNames = { "Repeat macro", "Delay", "Item pickup" };
        public const string DEFAULT_NAME = "Macro";

        public string Id { get; set; } = NewId();
        public ActionNodeKind Kind { get; set; } = ActionNodeKind.Macro;
        public string Name { get; set; } = "";
        // Top left corner on the canvas
        public double X { get; set; }
        public double Y { get; set; }
        // A "final" macro counts for the "Stop after N times" option of the Automation window
        public bool IsFinal { get; set; } = false;
        // A disabled macro is skipped: a link to it goes to another macro of its group, or to the End if there is none
        public bool Enabled { get; set; } = true;
        public MacroType Type { get; set; } = MacroType.Repeat;
        // Used by MacroType.Delay, in milliseconds
        public int DelayMs { get; set; } = 1000;
        // Used by MacroType.ItemPickup
        public ItemPickupSettings Pickup { get; set; } = ItemPickupSettings.CreateDefault();
        // Everything below is only used by MacroType.Repeat
        // Length of one loop in milliseconds (time from record start to record stop)
        public int Duration { get; set; }
        public List<MacroAction> Actions { get; set; } = new List<MacroAction>();

        [JsonIgnore]
        public bool IsMacro => Kind == ActionNodeKind.Macro;

        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                if (Kind == ActionNodeKind.Start) return "Start";
                if (Kind == ActionNodeKind.End) return "End";
                return Name == "" ? "(no name)" : Name;
            }
        }

        public static string NewId()
        {
            return Guid.NewGuid().ToString("N");
        }

        // Second line of the node on the canvas
        public string DescribeMacro()
        {
            if (Type == MacroType.Delay) return "Delay " + DelayMs + " ms";
            if (Type == MacroType.ItemPickup) return "Item pickup, " + Pickup.Labels.Count + " label color" + (Pickup.Labels.Count == 1 ? "" : "s");
            if (Actions.Count == 0) return "Repeat, nothing recorded";
            return "Repeat " + Actions.Count + " actions, " + (Duration / 1000.0).ToString("0.0") + " s loop";
        }
    }

    public enum ConditionType
    {
        // The macro played until its end
        MacroEnded,
        // An area of the screen looks like the saved sample
        ScreenMatch,
        // The macro played until its end a number of times
        MacroLooped,
        // The macro has been playing for some time. Checked at the end of each loop
        MacroRanFor
    }

    // The condition of a link: when it is met, the action goes to the target of the link
    public class ActionCondition
    {
        // Shown in the condition dropdown of the Automation window, in the order of ConditionType
        public static readonly string[] TypeNames =
        {
            "This macro finished (played once)",
            "An area of the screen matches a sample",
            "This macro has looped for a number of times",
            "This macro has run for a number of seconds"
        };

        public ConditionType Type { get; set; } = ConditionType.MacroEnded;

        // Used by ConditionType.MacroLooped
        public int LoopCount { get; set; } = 5;
        // Used by ConditionType.MacroRanFor
        public double RunSeconds { get; set; } = 60;

        // Everything below is only used by ConditionType.ScreenMatch
        public int CheckInterval { get; set; } = 500;
        // How far each of R, G, B can be from the sample for a pixel to count as matching
        public int ColorTolerance { get; set; } = 20;
        public int MatchPercent { get; set; } = 90;
        // Where the sample was cropped from, in screen pixels. Only this area is captured when running
        public int RegionX { get; set; }
        public int RegionY { get; set; }
        public int RegionWidth { get; set; }
        public int RegionHeight { get; set; }
        // The sample picture, as a base64 PNG
        public string SampleBase64 { get; set; } = "";

        [JsonIgnore]
        public System.Drawing.Rectangle Region => new System.Drawing.Rectangle(RegionX, RegionY, RegionWidth, RegionHeight);

        [JsonIgnore]
        public bool NeedsSample => Type == ConditionType.ScreenMatch && string.IsNullOrEmpty(SampleBase64);

        public ActionCondition Clone()
        {
            return (ActionCondition)MemberwiseClone();
        }

        public string Describe()
        {
            if (Type == ConditionType.MacroEnded) return "Macro finished";
            if (Type == ConditionType.MacroLooped) return "Macro has looped " + LoopCount + " times";
            if (Type == ConditionType.MacroRanFor) return "Macro has run for " + RunSeconds.ToString("0.#") + " seconds";
            return "Every " + CheckInterval + " ms, screen at (" + RegionX + ", " + RegionY + ") " + RegionWidth + "x" + RegionHeight
                + " matches sample >= " + MatchPercent + "%";
        }

        // The label of the link on the canvas
        public string DescribeShort()
        {
            if (Type == ConditionType.MacroEnded) return "finished";
            if (Type == ConditionType.MacroLooped) return "looped " + LoopCount + "x";
            if (Type == ConditionType.MacroRanFor) return "ran " + RunSeconds.ToString("0.#") + " s";
            return "screen " + MatchPercent + "%" + (NeedsSample ? " (no sample!)" : "");
        }
    }

    public class ActionLink
    {
        public string Id { get; set; } = ActionNode.NewId();
        public string FromId { get; set; } = "";
        public string ToId { get; set; } = "";
        // Null for the link from the Start node, which is followed right away
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ActionCondition Condition { get; set; } = new ActionCondition();
    }

    public class ActionPreset
    {
        public string Name { get; set; } = "";
        public List<ActionNode> Nodes { get; set; } = new List<ActionNode>();
        public List<ActionLink> Links { get; set; } = new List<ActionLink>();
        // Only found in files saved by older versions. Turned into nodes and links by MacroPresetStore.Load
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<LegacyActionStep> Steps { get; set; }

        [JsonIgnore]
        public ActionNode StartNode => Nodes.FirstOrDefault(n => n.Kind == ActionNodeKind.Start);
        [JsonIgnore]
        public ActionNode EndNode => Nodes.FirstOrDefault(n => n.Kind == ActionNodeKind.End);
        [JsonIgnore]
        public IEnumerable<ActionNode> MacroNodes => Nodes.Where(n => n.IsMacro);

        public ActionNode FindNode(string id)
        {
            return Nodes.FirstOrDefault(n => n.Id == id);
        }

        // In the order they are checked
        public List<ActionLink> LinksFrom(string nodeId)
        {
            return Links.Where(l => l.FromId == nodeId).ToList();
        }

        // The macros sharing the name of this node, itself included. A link to one of them goes to any of them,
        // picked at random. A node without a name, or the Start / End, is alone in its group
        public List<ActionNode> GroupOf(ActionNode node)
        {
            if (node == null) return new List<ActionNode>();
            if (!node.IsMacro || node.Name == "") return new List<ActionNode> { node };
            return Nodes.Where(n => n.IsMacro && string.Equals(n.Name, node.Name, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Where a link to this node really goes: the enabled macros of its group. Empty means the End
        public List<ActionNode> EnabledTargets(ActionNode node)
        {
            return GroupOf(node).Where(n => !n.IsMacro || n.Enabled).ToList();
        }

        // What a link to this node goes to, for the lists of the window
        public string DescribeTarget(ActionNode node)
        {
            if (node == null) return "?";
            int count = GroupOf(node).Count;
            int enabled = EnabledTargets(node).Count;
            if (enabled == 0) return node.DisplayName + " (disabled, ends the action)";
            if (count == 1) return node.DisplayName;
            return node.DisplayName + " (any of the " + enabled + (enabled < count ? " enabled" : "") + ")";
        }

        // Every action has a Start and an End node, they can't be deleted
        public void EnsureStartEnd()
        {
            if (StartNode == null) Nodes.Insert(0, new ActionNode { Kind = ActionNodeKind.Start, X = 40, Y = 200 });
            if (EndNode == null) Nodes.Add(new ActionNode { Kind = ActionNodeKind.End, X = 640, Y = 200 });
        }

        // Drop the links pointing nowhere, and the conditions that don't belong (a file can be edited by hand)
        public void Normalize()
        {
            EnsureStartEnd();
            ActionNode start = StartNode;
            Links.RemoveAll(l => FindNode(l.FromId) == null || FindNode(l.ToId) == null
                || l.ToId == start.Id || FindNode(l.FromId).Kind == ActionNodeKind.End);
            // The Start node has a single link, without any condition
            bool startLinked = false;
            for (int i = Links.Count - 1; i >= 0; i--)
            {
                ActionLink link = Links[i];
                if (link.FromId == start.Id)
                {
                    if (startLinked) Links.RemoveAt(i);
                    startLinked = true;
                    link.Condition = null;
                }
                else
                {
                    link.Condition ??= new ActionCondition();
                }
            }
        }

        public string UniqueName(string baseName)
        {
            string name = baseName;
            for (int i = 2; Nodes.Any(n => n.IsMacro && string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase)); i++)
            {
                name = baseName + " " + i;
            }
            return name;
        }

        public ActionNode AddMacroNode(double x, double y)
        {
            ActionNode node = new ActionNode { Name = UniqueName(ActionNode.DEFAULT_NAME), X = x, Y = y };
            Nodes.Add(node);
            return node;
        }

        // The links of the node go with it
        public void RemoveNode(ActionNode node)
        {
            Nodes.Remove(node);
            Links.RemoveAll(l => l.FromId == node.Id || l.ToId == node.Id);
        }
    }



    // What older versions saved: a library of macro presets, and action presets made of a list of steps
    // referencing them by name. MacroPresetStore.Load turns them into nodes and links.
    public class LegacyMacroPreset
    {
        public string Name { get; set; } = "";
        public MacroType Type { get; set; } = MacroType.Repeat;
        public int DelayMs { get; set; } = 1000;
        public ItemPickupSettings Pickup { get; set; }
        public int Duration { get; set; }
        public List<MacroAction> Actions { get; set; }
    }

    public enum LegacyConditionTarget
    {
        NextStep,
        GoToStep,
        EndAction
    }

    public class LegacyActionCondition : ActionCondition
    {
        public LegacyConditionTarget Target { get; set; } = LegacyConditionTarget.NextStep;
        public int TargetStep { get; set; }
    }

    public class LegacyActionStep
    {
        public string MacroName { get; set; } = "";
        public List<LegacyActionCondition> Conditions { get; set; } = new List<LegacyActionCondition>();
    }



    public class MacroPresetStore
    {
        public string SelectedActionPreset { get; set; } = "";
        public List<ActionPreset> ActionPresets { get; set; } = new List<ActionPreset>();
        // "Stop after" option of the Automation window
        public bool StopAfterEnabled { get; set; } = false;
        public int StopAfterCount { get; set; } = 10;
        public bool SleepWhenDone { get; set; } = false;

        // Only found in files saved by older versions, see LegacyMacroPreset
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<LegacyMacroPreset> Presets { get; set; }

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private const string FILE_NAME = "AutomationPresets.json";
        private const string BACKUP_FILE_NAME = "AutomationPresets.before-nodes.json";

        // Next to the executable, so the presets travel with the tool
        public static string FilePath
        {
            get { return Path.Combine(AppContext.BaseDirectory, FILE_NAME); }
        }

        // Where the file was before it moved next to the executable
        private static string OldFilePath
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "POE1Tools", FILE_NAME); }
        }

        public ActionPreset FindAction(string name)
        {
            return ActionPresets.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        // Used to deep copy action presets, and to tell if one was modified
        public static string ToJson(ActionPreset preset)
        {
            return JsonSerializer.Serialize(preset, _jsonOptions);
        }

        public static T CloneJson<T>(T value)
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, _jsonOptions), _jsonOptions);
        }

        public static ItemPickupSettings ClonePickup(ItemPickupSettings settings)
        {
            return CloneJson(settings);
        }

        public static ActionNode CloneNode(ActionNode node)
        {
            return CloneJson(node);
        }

        // The copy is normalized, so it serializes exactly like a normalized original. Without it, the link from the
        // Start node gets a condition back (its property initializer runs when the JSON leaves it out)
        public static ActionPreset CloneAction(ActionPreset preset)
        {
            ActionPreset clone = CloneJson(preset);
            clone.Normalize();
            return clone;
        }

        public static MacroPresetStore Load()
        {
            try
            {
                // Bring the presets made by older versions along. The old file is left where it is
                if (!File.Exists(FilePath) && File.Exists(OldFilePath))
                {
                    File.Copy(OldFilePath, FilePath);
                }

                if (File.Exists(FilePath))
                {
                    MacroPresetStore store = JsonSerializer.Deserialize<MacroPresetStore>(File.ReadAllText(FilePath), _jsonOptions);
                    if (store != null)
                    {
                        store.ActionPresets ??= new List<ActionPreset>();
                        store.ActionPresets.RemoveAll(p => p == null);
                        bool migrated = MigrateLegacy(store);
                        foreach (ActionPreset preset in store.ActionPresets)
                        {
                            Repair(preset);
                        }
                        if (migrated)
                        {
                            // The old file is kept, in case the older version has to be used again
                            File.Copy(FilePath, Path.Combine(AppContext.BaseDirectory, BACKUP_FILE_NAME), true);
                            store.Save();
                        }
                        return store;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load automation presets:\n" + ex.Message, "Automation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return new MacroPresetStore();
        }

        // Fill what a hand edited file can leave out
        private static void Repair(ActionPreset preset)
        {
            preset.Name ??= "";
            preset.Nodes ??= new List<ActionNode>();
            preset.Links ??= new List<ActionLink>();
            preset.Nodes.RemoveAll(n => n == null);
            preset.Links.RemoveAll(l => l == null);
            foreach (ActionNode node in preset.Nodes)
            {
                node.Id ??= ActionNode.NewId();
                node.Name ??= "";
                node.Actions ??= new List<MacroAction>();
                node.Pickup ??= ItemPickupSettings.CreateDefault();
                node.Pickup.Labels ??= new List<LabelColor>();
                foreach (MacroAction action in node.Actions)
                {
                    ConvertLegacyPosition(action);
                }
            }
            foreach (ActionLink link in preset.Links)
            {
                link.Id ??= ActionNode.NewId();
                link.FromId ??= "";
                link.ToId ??= "";
            }
            preset.Normalize();
        }

        // Returns true if something from an older version was converted
        private static bool MigrateLegacy(MacroPresetStore store)
        {
            List<LegacyMacroPreset> presets = (store.Presets ?? new List<LegacyMacroPreset>()).Where(p => p != null).ToList();
            HashSet<LegacyMacroPreset> used = new HashSet<LegacyMacroPreset>();
            bool migrated = false;

            foreach (ActionPreset action in store.ActionPresets)
            {
                if (action.Steps == null) continue;
                MigrateSteps(action, presets, used);
                action.Steps = null;
                migrated = true;
            }

            if (store.Presets != null)
            {
                // The macro presets no step was using end up on their own canvas, so nothing is lost
                List<LegacyMacroPreset> unused = presets.Where(p => !used.Contains(p)).ToList();
                if (unused.Count > 0)
                {
                    string name = "Old macro presets";
                    for (int i = 2; store.FindAction(name) != null; i++) name = "Old macro presets " + i;

                    ActionPreset keep = new ActionPreset { Name = name };
                    keep.EnsureStartEnd();
                    for (int i = 0; i < unused.Count; i++)
                    {
                        ActionNode node = NodeFromPreset(unused[i]);
                        node.Name = keep.UniqueName(node.Name);
                        node.X = 40 + 230 * (i % 4);
                        node.Y = 300 + 100 * (i / 4);
                        keep.Nodes.Add(node);
                    }
                    store.ActionPresets.Add(keep);
                }
                store.Presets = null;
                migrated = true;
            }
            return migrated;
        }

        // A step becomes a macro node with a copy of its macro preset, and each of its conditions becomes a link
        private static void MigrateSteps(ActionPreset action, List<LegacyMacroPreset> presets, HashSet<LegacyMacroPreset> used)
        {
            // Room between the nodes for the labels of the links
            const int COLUMNS = 4;
            const double X_STEP = 280;
            const double Y_STEP = 150;

            action.Nodes = new List<ActionNode>();
            action.Links = new List<ActionLink>();
            List<LegacyActionStep> steps = action.Steps.Where(s => s != null).ToList();

            // Laid out in rows: the Start, the steps in their order, then the End
            Func<int, double> columnX = position => 40 + (position % COLUMNS) * X_STEP;
            Func<int, double> rowY = position => 40 + (position / COLUMNS) * Y_STEP;

            ActionNode start = new ActionNode { Kind = ActionNodeKind.Start, X = columnX(0), Y = rowY(0) + 9 };
            ActionNode end = new ActionNode { Kind = ActionNodeKind.End, X = columnX(steps.Count + 1), Y = rowY(steps.Count + 1) + 9 };
            action.Nodes.Add(start);

            List<ActionNode> stepNodes = new List<ActionNode>();
            for (int i = 0; i < steps.Count; i++)
            {
                LegacyActionStep step = steps[i];
                string macroName = step.MacroName ?? "";
                LegacyMacroPreset preset = presets.FirstOrDefault(p => string.Equals(p.Name, macroName, StringComparison.OrdinalIgnoreCase));
                ActionNode node;
                if (preset != null)
                {
                    node = NodeFromPreset(preset);
                    used.Add(preset);
                }
                else
                {
                    // A step without a macro only waited for its conditions: a "Repeat" with nothing recorded does the same
                    node = new ActionNode { Name = macroName == "" ? "Wait" : macroName + " (missing)" };
                }
                node.Name = action.UniqueName(node.Name);
                node.X = columnX(i + 1);
                node.Y = rowY(i + 1);
                node.IsFinal = i == steps.Count - 1;
                action.Nodes.Add(node);
                stepNodes.Add(node);
            }
            action.Nodes.Add(end);

            action.Links.Add(new ActionLink { FromId = start.Id, ToId = stepNodes.Count > 0 ? stepNodes[0].Id : end.Id, Condition = null });
            for (int i = 0; i < steps.Count; i++)
            {
                foreach (LegacyActionCondition condition in steps[i].Conditions ?? new List<LegacyActionCondition>())
                {
                    if (condition == null) continue;

                    string toId = end.Id;
                    if (condition.Target == LegacyConditionTarget.NextStep && i + 1 < stepNodes.Count) toId = stepNodes[i + 1].Id;
                    if (condition.Target == LegacyConditionTarget.GoToStep && condition.TargetStep >= 0 && condition.TargetStep < stepNodes.Count) toId = stepNodes[condition.TargetStep].Id;

                    // The JSON round trip drops the legacy target
                    action.Links.Add(new ActionLink { FromId = stepNodes[i].Id, ToId = toId, Condition = CloneJson<ActionCondition>(condition) });
                }
            }
        }

        private static ActionNode NodeFromPreset(LegacyMacroPreset preset)
        {
            ActionNode node = new ActionNode
            {
                Name = preset.Name ?? "",
                Type = preset.Type,
                DelayMs = preset.DelayMs,
                Pickup = preset.Pickup == null ? ItemPickupSettings.CreateDefault() : CloneJson(preset.Pickup),
                Duration = preset.Duration
            };
            foreach (MacroAction action in preset.Actions ?? new List<MacroAction>())
            {
                node.Actions.Add(action.Clone());
            }
            return node;
        }

        // Older versions saved the click positions in pixels. The resolution they were recorded on
        // is unknown, the current one is the best guess
        private static void ConvertLegacyPosition(MacroAction action)
        {
            if (action.X == null && action.Y == null) return;

            // Key presses have no position, theirs was always (0, 0)
            if (action.IsMouse)
            {
                System.Drawing.Size screen = Screen.PrimaryScreen.Bounds.Size;
                action.XPercent = InputHook.PixelToPercent(action.X ?? 0, screen.Width);
                action.YPercent = InputHook.PixelToPercent(action.Y ?? 0, screen.Height);
            }
            action.X = null;
            action.Y = null;
        }

        public bool Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this, _jsonOptions));
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save automation presets:\n" + ex.Message, "Automation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
