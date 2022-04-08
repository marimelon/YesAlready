using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;

using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace YesAlready.Interface
{
    /// <summary>
    /// Plugin configuration window.
    /// </summary>
    internal class ConfigWindow : Window
    {
        private static readonly string AssemblyVersion = Assembly.GetAssembly(typeof(YesAlreadyPlugin))!.GetName().Version!.ToString();

        private readonly Vector4 shadedColor = new(0.68f, 0.68f, 0.68f, 1.0f);

        private readonly string[] hotkeyChoices = new[]
        {
            "None",
            "Control",
            "Alt",
            "Shift",
        };

        private readonly VirtualKey[] hotkeyValues = new[]
        {
            VirtualKey.NO_KEY,
            VirtualKey.CONTROL,
            VirtualKey.MENU,
            VirtualKey.SHIFT,
        };

        private ITextNode? draggedNode = null;
        private string debugClickName = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigWindow"/> class.
        /// </summary>
        public ConfigWindow()
            : base($"Yes Already {AssemblyVersion}")
        {
            this.Size = new Vector2(525, 600);
            this.SizeCondition = ImGuiCond.FirstUseEver;
        }

        private static TextFolderNode RootFolder => Service.Configuration.RootFolder;

        private static TextFolderNode ListRootFolder => Service.Configuration.ListRootFolder;

        private static TextFolderNode TalkRootFolder => Service.Configuration.TalkRootFolder;

        /// <inheritdoc/>
        public override void PreDraw()
        {
            ImGui.PushStyleColor(ImGuiCol.ResizeGrip, 0);
        }

        /// <inheritdoc/>
        public override void PostDraw()
        {
            ImGui.PopStyleColor();
        }

        /// <inheritdoc/>
        public override void Draw()
        {
#if DEBUG
            this.UiBuilder_TestButton();
#endif

            var enabled = Service.Configuration.Enabled;
            if (ImGui.Checkbox("Enabled", ref enabled))
            {
                Service.Configuration.Enabled = enabled;
                Service.Configuration.Save();
            }

            if (ImGui.BeginTabBar("Settings"))
            {
                this.DisplayTextOptions();
                this.DisplayListOptions();
                this.DisplayTalkOptions();
                this.DisplayBotherOptions();

                ImGui.EndTabBar();
            }
        }

        #region Testing

        private void UiBuilder_TestButton()
        {
            ImGui.InputText("ClickName", ref this.debugClickName, 100);
            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Check, "Submit"))
            {
                try
                {
                    this.debugClickName ??= string.Empty;
                    ClickLib.Click.SendClick(this.debugClickName.Trim());
                    Service.Plugin.PrintMessage($"Clicked {this.debugClickName} successfully.");
                }
                catch (ClickLib.ClickNotFoundError ex)
                {
                    Service.Plugin.PrintError(ex.Message);
                }
                catch (ClickLib.InvalidClickException ex)
                {
                    Service.Plugin.PrintError(ex.Message);
                }
                catch (Exception ex)
                {
                    Service.Plugin.PrintError(ex.Message);
                }
            }
        }

        #endregion

        // ====================================================================================================

        private void DisplayTextOptions()
        {
            if (!ImGui.BeginTabItem("YesNo"))
                return;

            ImGui.PushID("TextOptions");

            this.DisplayTextButtons();
            this.DisplayTextNodes();

            ImGui.PopID();

            ImGui.EndTabItem();
        }

        private void DisplayListOptions()
        {
            if (!ImGui.BeginTabItem("Lists"))
                return;

            ImGui.PushID("ListOptions");

            this.DisplayListButtons();
            this.DisplayListNodes();

            ImGui.PopID();

            ImGui.EndTabItem();
        }

        private void DisplayTalkOptions()
        {
            if (!ImGui.BeginTabItem("Talk"))
                return;

            ImGui.PushID("TalkOptions");

            this.DisplayTalkButtons();
            this.DisplayTalkNodes();

            ImGui.PopID();

            ImGui.EndTabItem();
        }

        private void DisplayBotherOptions()
        {
            if (!ImGui.BeginTabItem("Bothers"))
                return;

            static void IndentedTextColored(Vector4 color, string text)
            {
                var indent = 27f * ImGuiHelpers.GlobalScale;
                ImGui.Indent(indent);
                ImGui.PushStyleColor(ImGuiCol.Text, color);
                ImGui.TextWrapped(text);
                ImGui.PopStyleColor();
                ImGui.Unindent(indent);
            }

            ImGui.PushID("BotherOptions");

            #region Disable hotkey

            if (!this.hotkeyValues.Contains(Service.Configuration.DisableKey))
            {
                Service.Configuration.DisableKey = VirtualKey.NO_KEY;
                Service.Configuration.Save();
            }

            var disableHotkeyIndex = Array.IndexOf(this.hotkeyValues, Service.Configuration.DisableKey);

            ImGui.SetNextItemWidth(85);
            if (ImGui.Combo("Disable Hotkey", ref disableHotkeyIndex, this.hotkeyChoices, this.hotkeyChoices.Length))
            {
                Service.Configuration.DisableKey = this.hotkeyValues[disableHotkeyIndex];
                Service.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, $"While this key is held, the plugin is disabled.");

            #endregion
            #region Forced Yes hotkey

            if (!this.hotkeyValues.Contains(Service.Configuration.ForcedYesKey))
            {
                Service.Configuration.ForcedYesKey = VirtualKey.NO_KEY;
                Service.Configuration.Save();
            }

            var forcedYesHotkeyIndex = Array.IndexOf(this.hotkeyValues, Service.Configuration.ForcedYesKey);

            ImGui.SetNextItemWidth(85);
            if (ImGui.Combo("Forced Yes Hotkey", ref forcedYesHotkeyIndex, this.hotkeyChoices, this.hotkeyChoices.Length))
            {
                Service.Configuration.ForcedYesKey = this.hotkeyValues[forcedYesHotkeyIndex];
                Service.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, $"While this key is held, any Yes/No prompt will always default to yes. Be careful.");

            #endregion
            #region SalvageDialog

            var desynthDialog = Service.Configuration.DesynthDialogEnabled;
            if (ImGui.Checkbox("SalvageDialog", ref desynthDialog))
            {
                Service.Configuration.DesynthDialogEnabled = desynthDialog;
                Service.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Remove the Desynthesis menu confirmation.");

            #endregion
            #region SalvageDialog (Bulk)

            var desynthBulkDialog = Service.Configuration.DesynthBulkDialogEnabled;
            if (ImGui.Checkbox("SalvageDialog (Bulk)", ref desynthBulkDialog))
            {
                Service.Configuration.DesynthBulkDialogEnabled = desynthBulkDialog;
                Service.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Check the bulk desynthesis button when using the SalvageDialog feature.");

            #endregion
            #region MaterializeDialog

            var materialize = Service.Configuration.MaterializeDialogEnabled;
            if (ImGui.Checkbox("MaterializeDialog", ref materialize))
            {
                Service.Configuration.MaterializeDialogEnabled = materialize;
                Service.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Remove the create new materia confirmation.");

            #endregion
            #region MateriaRetrieveDialog

            var materiaRetrieve = Service.Configuration.MateriaRetrieveDialogEnabled;
            if (ImGui.Checkbox("MateriaRetrieveDialog", ref materiaRetrieve))
            {
                Service.Configuration.MateriaRetrieveDialogEnabled = materiaRetrieve;
                Service.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Remove the retrieve materia confirmation.");

            #endregion
            #region ItemInspectionResult

            var itemInspection = Service.Configuration.ItemInspectionResultEnabled;
            if (ImGui.Checkbox("ItemInspectionResult", ref itemInspection))
            {
                Service.Configuration.ItemInspectionResultEnabled = itemInspection;
                Service.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Eureka/Bozja lockboxes, forgotten fragments, and more.\nWarning: this does not check if you are maxed on items.");

            IndentedTextColored(this.shadedColor, "Rate limiter (pause after N items)");
            ImGui.SameLine();

            ImGui.PushItemWidth(100f * ImGuiHelpers.GlobalScale);
            var itemInspectionResultLimiter = Service.Configuration.ItemInspectionResultRateLimiter;
            if (ImGui.InputInt("###itemInspectionResultRateLimiter", ref itemInspectionResultLimiter))
            {
                if (itemInspectionResultLimiter < 0)
                {
                    itemInspectionResultLimiter = 0;
                }
                else
                {
                    Service.Configuration.ItemInspectionResultRateLimiter = itemInspectionResultLimiter;
                    Service.Configuration.Save();
                }
            }

            #endregion
            #region RetainerTaskAsk

            var retainerTaskAsk = Service.Configuration.RetainerTaskAskEnabled;
            if (ImGui.Checkbox("RetainerTaskAsk", ref retainerTaskAsk))
            {
                Service.Configuration.RetainerTaskAskEnabled = retainerTaskAsk;
                Service.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Skip the confirmation in the final dialog before sending out a retainer.");

            #endregion
            #region RetainerTaskResult

            var retainerTaskResult = Service.Configuration.RetainerTaskResultEnabled;
            if (ImGui.Checkbox("RetainerTaskResult", ref retainerTaskResult))
            {
                Service.Configuration.RetainerTaskResultEnabled = retainerTaskResult;
                Service.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Automatically send a retainer on the same venture as before when receiving an item.");

            #endregion
            #region GrandCompanySupplyReward

            var grandCompanySupplyReward = Service.Configuration.GrandCompanySupplyReward;
            if (ImGui.Checkbox("GrandCompanySupplyReward", ref grandCompanySupplyReward))
            {
                Service.Configuration.GrandCompanySupplyReward = grandCompanySupplyReward;
                Service.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Skip the confirmation when submitting Grand Company expert delivery items.");

            #endregion
            #region ShopCardDialog

            var shopCard = Service.Configuration.ShopCardDialog;
            if (ImGui.Checkbox("ShopCardDialog", ref shopCard))
            {
                Service.Configuration.ShopCardDialog = shopCard;
                Service.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Automatically confirm selling Triple Triad cards in the saucer.");

            #endregion
            #region JournalResultComplete

            var journalResultComplete = Service.Configuration.JournalResultCompleteEnabled;
            if (ImGui.Checkbox("JournalResultComplete", ref journalResultComplete))
            {
                Service.Configuration.JournalResultCompleteEnabled = journalResultComplete;
                Service.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Automatically confirm quest reward acceptance when there is nothing to choose.");

            #endregion
            #region ContentFinderConfirm

            var contentsFinderConfirm = Service.Configuration.ContentsFinderConfirmEnabled;
            if (ImGui.Checkbox("ContentsFinderConfirm", ref contentsFinderConfirm))
            {
                Service.Configuration.ContentsFinderConfirmEnabled = contentsFinderConfirm;

                if (!contentsFinderConfirm)
                    Service.Configuration.ContentsFinderOneTimeConfirmEnabled = false;

                Service.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Automatically commence duties when ready.");

            #endregion
            #region ContentFinderOneTimeConfirm

            var contentsFinderOneTimeConfirm = Service.Configuration.ContentsFinderOneTimeConfirmEnabled;
            if (ImGui.Checkbox("ContentsFinderOneTimeConfirm", ref contentsFinderOneTimeConfirm))
            {
                Service.Configuration.ContentsFinderOneTimeConfirmEnabled = contentsFinderOneTimeConfirm;

                if (contentsFinderOneTimeConfirm)
                    Service.Configuration.ContentsFinderConfirmEnabled = true;

                Service.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Automatically commence duties when ready, but only once.\nRequires Contents Finder Confirm, and disables both after activation.");

            #endregion
            #region ContextIconMenu
            var contextIconMenu = Service.Configuration.ContextIconMenuEnabled;
            if (ImGui.Checkbox("ContextIconMenu", ref contextIconMenu))
            {
                Service.Configuration.ContextIconMenuEnabled = contextIconMenu;
                Service.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Automatically select item when there is only one item.");
            #endregion
            ImGui.PopID();

            ImGui.EndTabItem();
        }

        // ====================================================================================================

        private void DisplayTextButtons()
        {
            var style = ImGui.GetStyle();
            var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

            if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
            {
                var newNode = new TextEntryNode { Enabled = false, Text = "Your text goes here" };
                RootFolder.Children.Add(newNode);
                Service.Configuration.Save();
            }

            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last seen as new entry"))
            {
                var io = ImGui.GetIO();
                var zoneRestricted = io.KeyCtrl;
                var createFolder = io.KeyShift;
                var selectNo = io.KeyAlt;

                Service.Configuration.CreateTextNode(RootFolder, zoneRestricted, createFolder, selectNo);
                Service.Configuration.Save();
            }

            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
            {
                var newNode = new TextFolderNode { Name = "Untitled folder" };
                RootFolder.Children.Add(newNode);
                Service.Configuration.Save();
            }

            var sb = new StringBuilder();
            sb.AppendLine("Enter into the input all or part of the text inside a dialog.");
            sb.AppendLine("For example: \"Teleport to \" for the teleport dialog.");
            sb.AppendLine();
            sb.AppendLine("Alternatively, wrap your text in forward slashes to use as a regex.");
            sb.AppendLine("As such: \"/Teleport to .*? for \\d+(,\\d+)? gil\\?/\"");
            sb.AppendLine("Or simpler: \"/Teleport to .*?/\" (and hope it doesn't match something unexpected)");
            sb.AppendLine();
            sb.AppendLine("If it matches, the yes button (and checkbox if present) will be clicked.");
            sb.AppendLine();
            sb.AppendLine("Right click a line to view options.");
            sb.AppendLine("Double click an entry for quick enable/disable.");
            sb.AppendLine("Ctrl-Shift right click a line to delete it and any children.");
            sb.AppendLine();
            sb.AppendLine("\"Add last seen as new entry\" button modifiers:");
            sb.AppendLine("   Shift-Click to add to a new or first existing folder with the current zone name, restricted to that zone.");
            sb.AppendLine("   Ctrl-Click to create a entry restricted to the current zone, without a named folder.");
            sb.AppendLine("   Alt-Click to create a \"Select No\" entry instead of \"Select Yes\"");
            sb.AppendLine("   Alt-Click can be combined with Shift/Ctrl-Click.");
            sb.AppendLine();
            sb.AppendLine("Currently supported text addons:");
            sb.AppendLine("  - SelectYesNo");

            ImGui.SameLine();
            ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());

            ImGui.PopStyleVar(); // ItemSpacing
        }

        private void DisplayTextNodes()
        {
            var root = RootFolder;
            this.TextNodeDragDrop(root);

            if (root.Children.Count == 0)
            {
                root.Children.Add(new TextEntryNode() { Enabled = false, Text = "Add some text here!" });
                Service.Configuration.Save();
            }

            foreach (var node in root.Children.ToArray())
            {
                this.DisplayTextNode(node, root);
            }
        }

        // ====================================================================================================

        private void DisplayListButtons()
        {
            var style = ImGui.GetStyle();
            var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

            if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
            {
                var newNode = new ListEntryNode { Enabled = false, Text = "Your text goes here" };
                ListRootFolder.Children.Add(newNode);
                Service.Configuration.Save();
            }

            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last selected as new entry"))
            {
                var newNode = new ListEntryNode { Enabled = true, Text = Service.Plugin.LastSeenListSelection, TargetRestricted = true, TargetText = Service.Plugin.LastSeenListTarget };
                ListRootFolder.Children.Add(newNode);
                Service.Configuration.Save();
            }

            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
            {
                var newNode = new TextFolderNode { Name = "Untitled folder" };
                ListRootFolder.Children.Add(newNode);
                Service.Configuration.Save();
            }

            var sb = new StringBuilder();
            sb.AppendLine("Enter into the input all or part of the text inside a line in a list dialog.");
            sb.AppendLine("For example: \"Purchase a Mini Cactpot ticket\" in the Gold Saucer.");
            sb.AppendLine();
            sb.AppendLine("Alternatively, wrap your text in forward slashes to use as a regex.");
            sb.AppendLine("As such: \"/Purchase a .*? ticket/\"");
            sb.AppendLine();
            sb.AppendLine("If any line in the list matches, then that line will be chosen.");
            sb.AppendLine();
            sb.AppendLine("Right click a line to view options.");
            sb.AppendLine("Double click an entry for quick enable/disable.");
            sb.AppendLine("Ctrl-Shift right click a line to delete it and any children.");
            sb.AppendLine();
            sb.AppendLine("Currently supported list addons:");
            sb.AppendLine("  - SelectString");
            sb.AppendLine("  - SelectIconString");

            ImGui.SameLine();
            ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());

            ImGui.PopStyleVar(); // ItemSpacing
        }

        private void DisplayListNodes()
        {
            var root = ListRootFolder;
            this.TextNodeDragDrop(root);

            if (root.Children.Count == 0)
            {
                root.Children.Add(new ListEntryNode() { Enabled = false, Text = "Add some text here!" });
                Service.Configuration.Save();
            }

            foreach (var node in root.Children.ToArray())
            {
                this.DisplayTextNode(node, root);
            }
        }

        // ====================================================================================================

        private void DisplayTalkButtons()
        {
            var style = ImGui.GetStyle();
            var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

            if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
            {
                var newNode = new TalkEntryNode { Enabled = false, TargetText = "Your text goes here" };
                TalkRootFolder.Children.Add(newNode);
                Service.Configuration.Save();
            }

            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add current target as a new entry"))
            {
                var target = Service.TargetManager.Target;
                var targetName = Service.Plugin.LastSeenTalkTarget = target != null
                    ? Service.Plugin.GetSeStringText(target.Name)
                    : string.Empty;

                var newNode = new TalkEntryNode { Enabled = true, TargetText = targetName };
                TalkRootFolder.Children.Add(newNode);
                Service.Configuration.Save();
            }

            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
            {
                var newNode = new TextFolderNode { Name = "Untitled folder" };
                TalkRootFolder.Children.Add(newNode);
                Service.Configuration.Save();
            }

            var sb = new StringBuilder();
            sb.AppendLine("Enter into the input all or part of the selected taret name while in a talk dialog.");
            sb.AppendLine("For example: \"Moyce\" in the Crystarium.");
            sb.AppendLine();
            sb.AppendLine("Alternatively, wrap your text in forward slashes to use as a regex.");
            sb.AppendLine("As such: \"/(Moyce|Eirikur)/\"");
            sb.AppendLine();
            sb.AppendLine("To skip your retainers, add the summoning bell.");
            sb.AppendLine();
            sb.AppendLine("Right click a line to view options.");
            sb.AppendLine("Double click an entry for quick enable/disable.");
            sb.AppendLine("Ctrl-Shift right click a line to delete it and any children.");
            sb.AppendLine();
            sb.AppendLine("Currently supported list addons:");
            sb.AppendLine("  - Talk");

            ImGui.SameLine();
            ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());

            ImGui.PopStyleVar(); // ItemSpacing
        }

        private void DisplayTalkNodes()
        {
            var root = TalkRootFolder;
            this.TextNodeDragDrop(root);

            if (root.Children.Count == 0)
            {
                root.Children.Add(new TalkEntryNode() { Enabled = false, TargetText = "Add some text here!" });
                Service.Configuration.Save();
            }

            foreach (var node in root.Children.ToArray())
            {
                this.DisplayTextNode(node, root);
            }
        }

        // ====================================================================================================

        private void DisplayTextNode(ITextNode node, TextFolderNode rootNode)
        {
            if (node is TextFolderNode folderNode)
            {
                this.DisplayFolderNode(folderNode, rootNode);
            }
            else if (node is TextEntryNode textNode)
            {
                this.DisplayTextEntryNode(textNode);
            }
            else if (node is ListEntryNode listNode)
            {
                this.DisplayListEntryNode(listNode);
            }
            else if (node is TalkEntryNode talkNode)
            {
                this.DisplayTalkEntryNode(talkNode);
            }
        }

        private void DisplayTextEntryNode(TextEntryNode node)
        {
            var validRegex = (node.IsTextRegex && node.TextRegex != null) || !node.IsTextRegex;
            var validZone = !node.ZoneRestricted || (node.ZoneIsRegex && node.ZoneRegex != null) || !node.ZoneIsRegex;

            if (!node.Enabled && (!validRegex || !validZone))
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
            else if (!node.Enabled)
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
            else if (!validRegex || !validZone)
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));

            ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
            ImGui.TreePop();

            if (!node.Enabled || !validRegex || !validZone)
                ImGui.PopStyleColor();

            if (!validRegex && !validZone)
                ImGuiEx.TextTooltip("Invalid Text and Zone Regex");
            else if (!validRegex)
                ImGuiEx.TextTooltip("Invalid Text Regex");
            else if (!validZone)
                ImGuiEx.TextTooltip("Invalid Zone Regex");

            if (ImGui.IsItemHovered())
            {
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    node.Enabled = !node.Enabled;
                    Service.Configuration.Save();
                    return;
                }
                else if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                {
                    var io = ImGui.GetIO();
                    if (io.KeyCtrl && io.KeyShift)
                    {
                        if (Service.Configuration.TryFindParent(node, out var parent))
                        {
                            parent!.Children.Remove(node);
                            Service.Configuration.Save();
                        }

                        return;
                    }
                    else
                    {
                        ImGui.OpenPopup($"{node.GetHashCode()}-popup");
                    }
                }
            }

            this.TextNodePopup(node);
            this.TextNodeDragDrop(node);
        }

        private void DisplayListEntryNode(ListEntryNode node)
        {
            var validRegex = (node.IsTextRegex && node.TextRegex != null) || !node.IsTextRegex;
            var validTarget = !node.TargetRestricted || (node.TargetIsRegex && node.TargetRegex != null) || !node.TargetIsRegex;

            if (!node.Enabled && (!validRegex || !validTarget))
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
            else if (!node.Enabled)
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
            else if (!validRegex || !validTarget)
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));

            ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
            ImGui.TreePop();

            if (!node.Enabled || !validRegex || !validTarget)
                ImGui.PopStyleColor();

            if (!validRegex && !validTarget)
                ImGuiEx.TextTooltip("Invalid Text and Target Regex");
            else if (!validRegex)
                ImGuiEx.TextTooltip("Invalid Text Regex");
            else if (!validTarget)
                ImGuiEx.TextTooltip("Invalid Target Regex");

            if (ImGui.IsItemHovered())
            {
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    node.Enabled = !node.Enabled;
                    Service.Configuration.Save();
                    return;
                }
                else if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                {
                    var io = ImGui.GetIO();
                    if (io.KeyCtrl && io.KeyShift)
                    {
                        if (Service.Configuration.TryFindParent(node, out var parent))
                        {
                            parent!.Children.Remove(node);
                            Service.Configuration.Save();
                        }

                        return;
                    }
                    else
                    {
                        ImGui.OpenPopup($"{node.GetHashCode()}-popup");
                    }
                }
            }

            this.TextNodePopup(node);
            this.TextNodeDragDrop(node);
        }

        private void DisplayTalkEntryNode(TalkEntryNode node)
        {
            var validTarget = (node.TargetIsRegex && node.TargetRegex != null) || !node.TargetIsRegex;

            if (!node.Enabled && !validTarget)
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
            else if (!node.Enabled)
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
            else if (!validTarget)
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));

            ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
            ImGui.TreePop();

            if (!node.Enabled || !validTarget)
                ImGui.PopStyleColor();

            if (!validTarget)
                ImGuiEx.TextTooltip("Invalid Target Regex");

            if (ImGui.IsItemHovered())
            {
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    node.Enabled = !node.Enabled;
                    Service.Configuration.Save();
                    return;
                }
                else if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                {
                    var io = ImGui.GetIO();
                    if (io.KeyCtrl && io.KeyShift)
                    {
                        if (Service.Configuration.TryFindParent(node, out var parent))
                        {
                            parent!.Children.Remove(node);
                            Service.Configuration.Save();
                        }

                        return;
                    }
                    else
                    {
                        ImGui.OpenPopup($"{node.GetHashCode()}-popup");
                    }
                }
            }

            this.TextNodePopup(node);
            this.TextNodeDragDrop(node);
        }

        private void DisplayFolderNode(TextFolderNode node, TextFolderNode root)
        {
            var expanded = ImGui.TreeNodeEx($"{node.Name}##{node.GetHashCode()}-tree");

            if (ImGui.IsItemHovered())
            {
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                {
                    var io = ImGui.GetIO();
                    if (io.KeyCtrl && io.KeyShift)
                    {
                        if (Service.Configuration.TryFindParent(node, out var parent))
                        {
                            parent!.Children.Remove(node);
                            Service.Configuration.Save();
                        }

                        return;
                    }
                    else
                    {
                        ImGui.OpenPopup($"{node.GetHashCode()}-popup");
                    }
                }
            }

            this.TextNodePopup(node, root);
            this.TextNodeDragDrop(node);

            if (expanded)
            {
                foreach (var childNode in node.Children.ToArray())
                {
                    this.DisplayTextNode(childNode, root);
                }

                ImGui.TreePop();
            }
        }

        private void TextNodePopup(ITextNode node, TextFolderNode? root = null)
        {
            var style = ImGui.GetStyle();
            var newItemSpacing = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);

            if (ImGui.BeginPopup($"{node.GetHashCode()}-popup"))
            {
                if (node is TextEntryNode entryNode)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                    var enabled = entryNode.Enabled;
                    if (ImGui.Checkbox("Enabled", ref enabled))
                    {
                        entryNode.Enabled = enabled;
                        Service.Configuration.Save();
                    }

                    ImGui.SameLine(100f);
                    var isYes = entryNode.IsYes;
                    var title = isYes ? "Click Yes" : "Click No";
                    if (ImGui.Button(title))
                    {
                        entryNode.IsYes = !isYes;
                        Service.Configuration.Save();
                    }

                    var trashAltWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

                    ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
                    if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                    {
                        if (Service.Configuration.TryFindParent(node, out var parentNode))
                        {
                            parentNode!.Children.Remove(node);
                            Service.Configuration.Save();
                        }
                    }

                    var matchText = entryNode.Text;
                    if (ImGui.InputText($"##{node.Name}-matchText", ref matchText, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        entryNode.Text = matchText;
                        Service.Configuration.Save();
                    }

                    var zoneRestricted = entryNode.ZoneRestricted;
                    if (ImGui.Checkbox("Zone Restricted", ref zoneRestricted))
                    {
                        entryNode.ZoneRestricted = zoneRestricted;
                        Service.Configuration.Save();
                    }

                    var searchWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.Search);
                    var searchPlusWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

                    ImGui.SameLine(ImGui.GetContentRegionMax().X - searchWidth);
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Search, "Zone List"))
                    {
                        Service.Plugin.OpenZoneListUi();
                    }

                    ImGui.SameLine(ImGui.GetContentRegionMax().X - searchWidth - searchPlusWidth - newItemSpacing.X);
                    if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Fill with current zone"))
                    {
                        var currentID = Service.ClientState.TerritoryType;
                        if (Service.Plugin.TerritoryNames.TryGetValue(currentID, out var zoneName))
                        {
                            entryNode.ZoneText = zoneName;
                            Service.Configuration.Save();
                        }
                        else
                        {
                            entryNode.ZoneText = "Could not find name";
                            Service.Configuration.Save();
                        }
                    }

                    ImGui.PopStyleVar(); // ItemSpacing

                    var zoneText = entryNode.ZoneText;
                    if (ImGui.InputText($"##{node.Name}-zoneText", ref zoneText, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        entryNode.ZoneText = zoneText;
                        Service.Configuration.Save();
                    }
                }

                if (node is ListEntryNode listNode)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                    var enabled = listNode.Enabled;
                    if (ImGui.Checkbox("Enabled", ref enabled))
                    {
                        listNode.Enabled = enabled;
                        Service.Configuration.Save();
                    }

                    var trashAltWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

                    ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
                    if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                    {
                        if (Service.Configuration.TryFindParent(node, out var parentNode))
                        {
                            parentNode!.Children.Remove(node);
                            Service.Configuration.Save();
                        }
                    }

                    var matchText = listNode.Text;
                    if (ImGui.InputText($"##{node.Name}-matchText", ref matchText, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        listNode.Text = matchText;
                        Service.Configuration.Save();
                    }

                    var targetRestricted = listNode.TargetRestricted;
                    if (ImGui.Checkbox("Target Restricted", ref targetRestricted))
                    {
                        listNode.TargetRestricted = targetRestricted;
                        Service.Configuration.Save();
                    }

                    var searchPlusWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

                    ImGui.SameLine(ImGui.GetContentRegionMax().X - searchPlusWidth);
                    if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Fill with current target"))
                    {
                        var target = Service.TargetManager.Target;
                        var name = target?.Name?.TextValue ?? string.Empty;

                        if (!string.IsNullOrEmpty(name))
                        {
                            listNode.TargetText = name;
                            Service.Configuration.Save();
                        }
                        else
                        {
                            listNode.TargetText = "Could not find target";
                            Service.Configuration.Save();
                        }
                    }

                    ImGui.PopStyleVar(); // ItemSpacing

                    var targetText = listNode.TargetText;
                    if (ImGui.InputText($"##{node.Name}-targetText", ref targetText, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        listNode.TargetText = targetText;
                        Service.Configuration.Save();
                    }
                }

                if (node is TalkEntryNode talkNode)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                    var enabled = talkNode.Enabled;
                    if (ImGui.Checkbox("Enabled", ref enabled))
                    {
                        talkNode.Enabled = enabled;
                        Service.Configuration.Save();
                    }

                    var trashAltWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

                    ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
                    if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                    {
                        if (Service.Configuration.TryFindParent(node, out var parentNode))
                        {
                            parentNode!.Children.Remove(node);
                            Service.Configuration.Save();
                        }
                    }

                    var searchPlusWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

                    ImGui.SameLine(ImGui.GetContentRegionMax().X - searchPlusWidth - trashAltWidth - newItemSpacing.X);
                    if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Fill with current target"))
                    {
                        var target = Service.TargetManager.Target;
                        var name = target?.Name?.TextValue ?? string.Empty;

                        if (!string.IsNullOrEmpty(name))
                        {
                            talkNode.TargetText = name;
                            Service.Configuration.Save();
                        }
                        else
                        {
                            talkNode.TargetText = "Could not find target";
                            Service.Configuration.Save();
                        }
                    }

                    ImGui.PopStyleVar(); // ItemSpacing

                    var targetText = talkNode.TargetText;
                    if (ImGui.InputText($"##{node.Name}-targetText", ref targetText, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        talkNode.TargetText = targetText;
                        Service.Configuration.Save();
                    }
                }

                if (node is TextFolderNode folderNode)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                    if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add entry"))
                    {
                        if (root == RootFolder)
                        {
                            var newNode = new TextEntryNode { Enabled = false, Text = "Your text goes here" };
                            folderNode.Children.Add(newNode);
                        }
                        else if (root == ListRootFolder)
                        {
                            var newNode = new ListEntryNode { Enabled = false, Text = "Your text goes here" };
                            folderNode.Children.Add(newNode);
                        }

                        Service.Configuration.Save();
                    }

                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last seen as new entry"))
                    {
                        if (root == RootFolder)
                        {
                            var io = ImGui.GetIO();
                            var zoneRestricted = io.KeyCtrl;
                            var createFolder = io.KeyShift;
                            var selectNo = io.KeyAlt;

                            Service.Configuration.CreateTextNode(folderNode, zoneRestricted, createFolder, selectNo);
                            Service.Configuration.Save();
                        }
                        else if (root == ListRootFolder)
                        {
                            var newNode = new ListEntryNode() { Enabled = true, Text = Service.Plugin.LastSeenListSelection, TargetRestricted = true, TargetText = Service.Plugin.LastSeenListTarget };
                            folderNode.Children.Add(newNode);
                            Service.Configuration.Save();
                        }
                        else if (root == TalkRootFolder)
                        {
                            var newNode = new TalkEntryNode() { Enabled = true, TargetText = Service.Plugin.LastSeenTalkTarget };
                            folderNode.Children.Add(newNode);
                            Service.Configuration.Save();
                        }
                    }

                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
                    {
                        var newNode = new TextFolderNode { Name = "Untitled folder" };
                        folderNode.Children.Add(newNode);
                        Service.Configuration.Save();
                    }

                    var trashWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);
                    ImGui.SameLine(ImGui.GetContentRegionMax().X - trashWidth);
                    if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                    {
                        if (Service.Configuration.TryFindParent(node, out var parentNode))
                        {
                            parentNode!.Children.Remove(node);
                            Service.Configuration.Save();
                        }
                    }

                    ImGui.PopStyleVar(); // ItemSpacing

                    var folderName = folderNode.Name;
                    if (ImGui.InputText($"##{node.Name}-rename", ref folderName, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        folderNode.Name = folderName;
                        Service.Configuration.Save();
                    }
                }

                ImGui.EndPopup();
            }
        }

        private void TextNodeDragDrop(ITextNode node)
        {
            if (node != RootFolder && node != ListRootFolder && node != TalkRootFolder && ImGui.BeginDragDropSource())
            {
                this.draggedNode = node;

                ImGui.Text(node.Name);
                ImGui.SetDragDropPayload("TextNodePayload", IntPtr.Zero, 0);
                ImGui.EndDragDropSource();
            }

            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("TextNodePayload");

                bool nullPtr;
                unsafe
                {
                    nullPtr = payload.NativePtr == null;
                }

                var targetNode = node;
                if (!nullPtr && payload.IsDelivery() && this.draggedNode != null)
                {
                    if (Service.Configuration.TryFindParent(this.draggedNode, out var draggedNodeParent))
                    {
                        if (targetNode is TextFolderNode targetFolderNode)
                        {
                            draggedNodeParent!.Children.Remove(this.draggedNode);
                            targetFolderNode.Children.Add(this.draggedNode);
                            Service.Configuration.Save();
                        }
                        else
                        {
                            if (Service.Configuration.TryFindParent(targetNode, out var targetNodeParent))
                            {
                                var targetNodeIndex = targetNodeParent!.Children.IndexOf(targetNode);
                                if (targetNodeParent == draggedNodeParent)
                                {
                                    var draggedNodeIndex = targetNodeParent.Children.IndexOf(this.draggedNode);
                                    if (draggedNodeIndex < targetNodeIndex)
                                    {
                                        targetNodeIndex -= 1;
                                    }
                                }

                                draggedNodeParent!.Children.Remove(this.draggedNode);
                                targetNodeParent.Children.Insert(targetNodeIndex, this.draggedNode);
                                Service.Configuration.Save();
                            }
                            else
                            {
                                throw new Exception($"Could not find parent of node \"{targetNode.Name}\"");
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"Could not find parent of node \"{this.draggedNode.Name}\"");
                    }

                    this.draggedNode = null;
                }

                ImGui.EndDragDropTarget();
            }
        }
    }
}
