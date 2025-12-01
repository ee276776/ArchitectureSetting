using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace ArchitectureSetting
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            // 設定預設平台為 x64
            comboBoxPlatform.SelectedIndex = 0;

            // 加入日誌歡迎訊息
            AppendLog("架構設定工具已啟動");
            AppendLog("功能說明：");
            AppendLog("1. 新增指定的目標平台設定到方案和專案");
            AppendLog("2. 將現有專案組態中所有平台統一改為指定平台");
            AppendLog("請選擇方案資料夾並選擇目標平台");
        }

        private void buttonBrowseFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxSolutionPath.Text = folderBrowserDialog.SelectedPath;
                AppendLog($"已選擇方案資料夾: {folderBrowserDialog.SelectedPath}");

                // 檢查資料夾內是否有 .sln 檔案
                CheckSolutionFiles(folderBrowserDialog.SelectedPath);
            }
        }

        private void CheckSolutionFiles(string folderPath)
        {
            try
            {
                var slnFiles = Directory.GetFiles(folderPath, "*.sln");
                if (slnFiles.Length > 0)
                {
                    AppendLog($"找到 {slnFiles.Length} 個方案檔案:");
                    foreach (var sln in slnFiles)
                    {
                        AppendLog($"  - {Path.GetFileName(sln)}");
                    }
                }
                else
                {
                    AppendLog("警告: 在選擇的資料夾中未找到方案檔案 (.sln)");
                }
            }
            catch (Exception ex)
            {
                AppendLog($"檢查方案檔案時發生錯誤: {ex.Message}");
            }
        }

        private void buttonBatchProcess_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxSolutionPath.Text))
            {
                MessageBox.Show("請先選擇方案資料夾", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBoxPlatform.SelectedItem == null)
            {
                MessageBox.Show("請選擇目標平台", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string solutionPath = textBoxSolutionPath.Text;
            string targetPlatform = comboBoxPlatform.SelectedItem.ToString();

            AppendLog($"開始處理平台設定...");
            AppendLog($"目標平台: {targetPlatform}");
            AppendLog("功能：1. 新增平台設定  2. 將現有專案組態的平台都改為目標平台");

            // 禁用按鈕防止重複點擊
            buttonBatchProcess.Enabled = false;

            try
            {
                AddPlatformToSolution(solutionPath, targetPlatform);
            }
            catch (Exception ex)
            {
                AppendLog($"處理平台設定時發生錯誤: {ex.Message}");
            }
            finally
            {
                buttonBatchProcess.Enabled = true;
            }
        }

        private void AddPlatformToSolution(string solutionPath, string targetPlatform)
        {
            try
            {
                // 尋找方案檔案
                var slnFiles = Directory.GetFiles(solutionPath, "*.sln");

                if (slnFiles.Length == 0)
                {
                    AppendLog("錯誤: 找不到方案檔案");
                    return;
                }

                foreach (var slnFile in slnFiles)
                {
                    AppendLog($"處理方案: {Path.GetFileName(slnFile)}");

                    // 修改方案檔案
                    ModifySolutionFile(slnFile, targetPlatform);

                    // 取得方案中的專案並修改
                    var projects = GetProjectsFromSolution(slnFile);
                    foreach (var project in projects)
                    {
                        string projectPath = Path.Combine(Path.GetDirectoryName(slnFile), project);
                        if (File.Exists(projectPath))
                        {
                            ModifyProjectFile(projectPath, targetPlatform);
                        }
                    }
                }

                AppendLog("平台設定新增完成!");
            }
            catch (Exception ex)
            {
                AppendLog($"處理方案時發生錯誤: {ex.Message}");
            }
        }

        //private void ModifySolutionFile(string solutionFile, string platform)
        //{
        //    try
        //    {
        //        AppendLog($"修改方案檔案: {Path.GetFileName(solutionFile)}");

        //        string content = File.ReadAllText(solutionFile, Encoding.UTF8);
        //        string[] lines = content.Split('\n');
        //        List<string> newLines = new List<string>();

        //        bool inGlobalSection = false;
        //        bool inSolutionConfigurationPlatforms = false;
        //        bool inProjectConfigurationPlatforms = false;
        //        List<string> projectGuids = new List<string>();
        //        int modifiedCount = 0;

        //        // 先找出所有專案的 GUID
        //        foreach (string line in lines)
        //        {
        //            var projectMatch = Regex.Match(line, @"Project\(""{.*?}""\)\s*=\s*"".*?"",\s*"".*?"",\s*""\{(.*?)\}""");
        //            if (projectMatch.Success)
        //            {
        //                projectGuids.Add(projectMatch.Groups[1].Value);
        //            }
        //        }

        //        foreach (string line in lines)
        //        {
        //            string trimmedLine = line.Trim();

        //            if (trimmedLine.StartsWith("GlobalSection(SolutionConfigurationPlatforms)"))
        //            {
        //                inGlobalSection = true;
        //                inSolutionConfigurationPlatforms = true;
        //                newLines.Add(line);
        //                continue;
        //            }

        //            if (trimmedLine.StartsWith("GlobalSection(ProjectConfigurationPlatforms)"))
        //            {
        //                inGlobalSection = true;
        //                inProjectConfigurationPlatforms = true;
        //                newLines.Add(line);
        //                AppendLog($"開始處理 ProjectConfigurationPlatforms 區段");
        //                continue;
        //            }

        //            if (trimmedLine == "EndGlobalSection")
        //            {
        //                if (inSolutionConfigurationPlatforms)
        //                {
        //                    // 新增解決方案組態平台 (保留原功能)
        //                    if (!content.Contains($"Debug|{platform}"))
        //                    {
        //                        newLines.Add($"\t\tDebug|{platform} = Debug|{platform}");
        //                        AppendLog($"新增方案組態: Debug|{platform}");
        //                    }
        //                    if (!content.Contains($"Release|{platform}"))
        //                    {
        //                        newLines.Add($"\t\tRelease|{platform} = Release|{platform}");
        //                        AppendLog($"新增方案組態: Release|{platform}");
        //                    }
        //                }

        //                if (inProjectConfigurationPlatforms)
        //                {
        //                    // 為每個專案新增平台組態 (保留原功能)
        //                    foreach (string guid in projectGuids)
        //                    {
        //                        bool hasDebugConfig = content.Contains($"{{{guid.ToUpper()}}}.Debug|{platform}.ActiveCfg");
        //                        bool hasReleaseConfig = content.Contains($"{{{guid.ToUpper()}}}.Release|{platform}.ActiveCfg");

        //                        if (!hasDebugConfig || !hasReleaseConfig)
        //                        {
        //                            if (!hasDebugConfig)
        //                            {
        //                                newLines.Add($"\t\t{{{guid.ToUpper()}}}.Debug|{platform}.ActiveCfg = Debug|{platform}");
        //                                newLines.Add($"\t\t{{{guid.ToUpper()}}}.Debug|{platform}.Build.0 = Debug|{platform}");
        //                            }
        //                            if (!hasReleaseConfig)
        //                            {
        //                                newLines.Add($"\t\t{{{guid.ToUpper()}}}.Release|{platform}.ActiveCfg = Release|{platform}");
        //                                newLines.Add($"\t\t{{{guid.ToUpper()}}}.Release|{platform}.Build.0 = Release|{platform}");
        //                            }
        //                            AppendLog($"為專案 {guid} 新增 {platform} 平台組態");
        //                        }
        //                    }

        //                    if (modifiedCount > 0)
        //                    {
        //                        AppendLog($"共修改了 {modifiedCount} 行專案組態，將平台統一改為: {platform}");
        //                    }
        //                }

        //                inSolutionConfigurationPlatforms = false;
        //                inProjectConfigurationPlatforms = false;
        //                inGlobalSection = false;
        //                newLines.Add(line);
        //                continue;
        //            }

        //            // 處理 ProjectConfigurationPlatforms 區段內的行 (新功能)
        //            if (inProjectConfigurationPlatforms && !trimmedLine.StartsWith("EndGlobalSection") && !string.IsNullOrWhiteSpace(trimmedLine))
        //            {
        //                // 修改等號後面的平台部分
        //                string modifiedLine = ModifyProjectConfigurationLine(line, platform);
        //                newLines.Add(modifiedLine);

        //                if (modifiedLine != line)
        //                {
        //                    modifiedCount++;
        //                    AppendLog($"  修改: {line.Trim()} -> {modifiedLine.Trim()}");
        //                }
        //                continue;
        //            }

        //            newLines.Add(line);
        //        }

        //        string finalContent = string.Join("\n", newLines);
        //        File.WriteAllText(solutionFile, finalContent, Encoding.UTF8);
        //        AppendLog($"方案檔案修改完成");
        //    }
        //    catch (Exception ex)
        //    {
        //        AppendLog($"修改方案檔案時發生錯誤: {ex.Message}");
        //    }
        //}
        private void ModifySolutionFile(string solutionFile, string platform)
        {
            try
            {
                AppendLog($"修改方案檔案: {Path.GetFileName(solutionFile)}");

                string content = File.ReadAllText(solutionFile, Encoding.UTF8);
                string[] lines = content.Split('\n');
                List<string> newLines = new List<string>();

                bool inSolutionConfigurationPlatforms = false;
                bool inProjectConfigurationPlatforms = false;
                int modifiedCount = 0;

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();

                    if (trimmedLine.StartsWith("GlobalSection(SolutionConfigurationPlatforms)"))
                    {
                        inSolutionConfigurationPlatforms = true;
                        newLines.Add(line);
                        AppendLog($"進入 SolutionConfigurationPlatforms 區段");
                        continue;
                    }

                    if (trimmedLine.StartsWith("GlobalSection(ProjectConfigurationPlatforms)"))
                    {
                        inProjectConfigurationPlatforms = true;
                        newLines.Add(line);
                        AppendLog($"進入 ProjectConfigurationPlatforms 區段");
                        continue;
                    }

                    if (trimmedLine == "EndGlobalSection")
                    {
                        if (inSolutionConfigurationPlatforms)
                        {
                            AppendLog($"結束 SolutionConfigurationPlatforms 區段");
                        }

                        if (inProjectConfigurationPlatforms)
                        {
                            AppendLog($"結束 ProjectConfigurationPlatforms 區段，共修改了 {modifiedCount} 行");
                        }

                        inSolutionConfigurationPlatforms = false;
                        inProjectConfigurationPlatforms = false;
                        newLines.Add(line);
                        continue;
                    }

                    // 處理 ProjectConfigurationPlatforms 區段內的行
                    if (inProjectConfigurationPlatforms && !string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        // 修改等號後面的平台部分
                        string modifiedLine = ModifyProjectConfigurationLine(line, platform);
                        newLines.Add(modifiedLine);

                        if (modifiedLine != line)
                        {
                            modifiedCount++;
                            AppendLog($"  修改: {trimmedLine}");
                            AppendLog($"    -> {modifiedLine.Trim()}");
                        }
                        continue;
                    }

                    // 其他行保持原樣
                    newLines.Add(line);
                }

                string finalContent = string.Join("\n", newLines);
                File.WriteAllText(solutionFile, finalContent, Encoding.UTF8);
                AppendLog($"方案檔案修改完成");
            }
            catch (Exception ex)
            {
                AppendLog($"修改方案檔案時發生錯誤: {ex.Message}");
            }
        }
        private string ModifyProjectConfigurationLine(string line, string targetPlatform)
        {
            try
            {
                // 匹配模式：{GUID}.Configuration|Platform.Setting = Configuration|Platform
                // 例如：{98C9DD25-8E02-498F-8128-681F9A6F8461}.Debug|Any CPU.ActiveCfg = Debug|Any CPU

                var pattern = @"(\s*\{[^}]+\}\.(Debug|Release)\|[^.]+\.(ActiveCfg|Build\.0))\s*=\s*(Debug|Release)\|(.+)";
                var match = Regex.Match(line, pattern);

                if (match.Success)
                {
                    string leftSide = match.Groups[1].Value;
                    string configuration = match.Groups[4].Value; // Debug 或 Release
                    string whitespace = "";

                    // 保持原有的縮排
                    int firstNonWhiteIndex = 0;
                    while (firstNonWhiteIndex < line.Length && char.IsWhiteSpace(line[firstNonWhiteIndex]))
                    {
                        firstNonWhiteIndex++;
                    }
                    if (firstNonWhiteIndex > 0)
                    {
                        whitespace = line.Substring(0, firstNonWhiteIndex);
                    }

                    // 構建新的行：保持左側不變，右側改為指定平台
                    string newLine = whitespace + leftSide.Trim() + " = " + configuration + "|" + targetPlatform;

                    return newLine;
                }

                return line; // 如果不匹配模式，保持原樣
            }
            catch (Exception ex)
            {
                AppendLog($"修改專案組態行時發生錯誤: {ex.Message}");
                return line;
            }
        }

        private List<string> GetProjectsFromSolution(string solutionFile)
        {
            var projects = new List<string>();

            try
            {
                string content = File.ReadAllText(solutionFile);
                var projectMatches = Regex.Matches(content, @"Project\(""{.*?}""\)\s*=\s*"".*?"",\s*""(.*?\.csproj)""");

                foreach (Match match in projectMatches)
                {
                    projects.Add(match.Groups[1].Value);
                }

                AppendLog($"找到 {projects.Count} 個專案");
            }
            catch (Exception ex)
            {
                AppendLog($"解析方案檔案時發生錯誤: {ex.Message}");
            }

            return projects;
        }

        //private void ModifyProjectFile(string projectPath, string platform)
        //{
        //    try
        //    {
        //        AppendLog($"修改專案檔案: {Path.GetFileName(projectPath)}");

        //        string content = File.ReadAllText(projectPath, Encoding.UTF8);

        //        // 檢查是否已經有該平台的設定
        //        if (content.Contains($"<Platform>{platform}</Platform>") ||
        //            content.Contains($"<Platforms>") && content.Contains(platform))
        //        {
        //            AppendLog($"專案 {Path.GetFileName(projectPath)} 已包含 {platform} 平台設定");
        //            return;
        //        }

        //        // 尋找 <Platforms> 標籤或 <Platform> 標籤
        //        if (content.Contains("<Platforms>"))
        //        {
        //            // 如果有 Platforms 標籤，添加到現有平台
        //            string platformsPattern = @"<Platforms>(.*?)</Platforms>";
        //            Match platformsMatch = Regex.Match(content, platformsPattern);

        //            if (platformsMatch.Success)
        //            {
        //                string currentPlatforms = platformsMatch.Groups[1].Value;
        //                if (!currentPlatforms.Contains(platform))
        //                {
        //                    string newPlatforms = currentPlatforms.TrimEnd(';') + ";" + platform;
        //                    content = content.Replace($"<Platforms>{currentPlatforms}</Platforms>",
        //                                            $"<Platforms>{newPlatforms}</Platforms>");
        //                    AppendLog($"新增 {platform} 到專案平台清單");
        //                }
        //            }
        //        }
        //        else
        //        {
        //            // 如果沒有 Platforms 標籤，在 PropertyGroup 中添加
        //            string propertyGroupPattern = @"(<PropertyGroup[^>]*>)";
        //            Match propertyGroupMatch = Regex.Match(content, propertyGroupPattern);

        //            if (propertyGroupMatch.Success)
        //            {
        //                string insertion = propertyGroupMatch.Groups[1].Value + Environment.NewLine +
        //                                 $"    <Platforms>AnyCPU;{platform}</Platforms>";
        //                content = content.Replace(propertyGroupMatch.Groups[1].Value, insertion);
        //                AppendLog($"新增專案平台設定: {platform}");
        //            }
        //        }

        //        File.WriteAllText(projectPath, content, Encoding.UTF8);
        //        AppendLog($"專案檔案 {Path.GetFileName(projectPath)} 修改完成");
        //    }
        //    catch (Exception ex)
        //    {
        //        AppendLog($"修改專案檔案 {Path.GetFileName(projectPath)} 時發生錯誤: {ex.Message}");
        //    }
        //}
        private void ModifyProjectFile(string projectPath, string platform)
        {
            try
            {
                AppendLog($"修改專案檔案: {Path.GetFileName(projectPath)}");
                string content = File.ReadAllText(projectPath, Encoding.UTF8);

                // ===== 步驟1: 處理 Platforms 標籤 =====
                if (content.Contains("<Platforms>"))
                {
                    // 如果有 Platforms 標籤，添加到現有平台
                    string platformsPattern = @"<Platforms>(.*?)</Platforms>";
                    Match platformsMatch = Regex.Match(content, platformsPattern);
                    if (platformsMatch.Success)
                    {
                        string currentPlatforms = platformsMatch.Groups[1].Value;
                        if (!currentPlatforms.Contains(platform))
                        {
                            string newPlatforms = currentPlatforms.TrimEnd(';') + ";" + platform;
                            content = content.Replace($"<Platforms>{currentPlatforms}</Platforms>",
                                                    $"<Platforms>{newPlatforms}</Platforms>");
                            AppendLog($"新增 {platform} 到專案平台清單");
                        }
                        else
                        {
                            AppendLog($"專案平台清單已包含 {platform}");
                        }
                    }
                }
                else
                {
                    // 如果沒有 Platforms 標籤，在第一個 PropertyGroup 中添加
                    string propertyGroupPattern = @"(<PropertyGroup>)";
                    Match propertyGroupMatch = Regex.Match(content, propertyGroupPattern);
                    if (propertyGroupMatch.Success)
                    {
                        int insertPos = propertyGroupMatch.Index + propertyGroupMatch.Length;
                        string insertion = Environment.NewLine + $"    <Platforms>AnyCPU;{platform}</Platforms>";
                        content = content.Insert(insertPos, insertion);
                        AppendLog($"新增專案平台設定: AnyCPU;{platform}");
                    }
                }

                // ===== 步驟2: 檢查是否已有 Debug|x64 和 Release|x64 的 PropertyGroup =====
                string debugCondition = $"'$(Configuration)|$(Platform)' == 'Debug|{platform}'";
                string releaseCondition = $"'$(Configuration)|$(Platform)' == 'Release|{platform}'";

                bool hasDebugConfig = content.Contains(debugCondition);
                bool hasReleaseConfig = content.Contains(releaseCondition);

                AppendLog($"檢查 PropertyGroup 設定:");
                AppendLog($"  Debug|{platform} 存在: {hasDebugConfig}");
                AppendLog($"  Release|{platform} 存在: {hasReleaseConfig}");

                if (hasDebugConfig && hasReleaseConfig)
                {
                    AppendLog($"專案 {Path.GetFileName(projectPath)} 已包含完整的 {platform} 平台 PropertyGroup 設定");
                }
                else
                {
                    // ===== 步驟3: 找到最後一個有 Condition="'$(Configuration)|$(Platform)'" 的 PropertyGroup 結尾 =====
                    // 使用更精確的模式來匹配
                    string pattern = @"<PropertyGroup\s+Condition\s*=\s*""\s*'\$\(Configuration\)\|\$\(Platform\)'\s*==\s*'[^']+'\s*""[^>]*>[\s\S]*?</PropertyGroup>";
                    MatchCollection matches = Regex.Matches(content, pattern);

                    if (matches.Count == 0)
                    {
                        AppendLog($"錯誤：找不到帶有 Condition 的 PropertyGroup");
                        AppendLog($"嘗試使用備用方法...");

                        // 備用方法：找第一個 <ItemGroup> 之前
                        int itemGroupPos = content.IndexOf("<ItemGroup>");
                        if (itemGroupPos > 0)
                        {
                            // 找到第一個 <ItemGroup> 前的最後一個 </PropertyGroup>
                            string beforeItemGroup = content.Substring(0, itemGroupPos);
                            int lastPropGroupEnd = beforeItemGroup.LastIndexOf("</PropertyGroup>");

                            if (lastPropGroupEnd > 0)
                            {
                                int insertPosition = lastPropGroupEnd + "</PropertyGroup>".Length;
                                AppendLog($"使用備用方法：在第一個 ItemGroup 前的最後一個 PropertyGroup 後插入");

                                StringBuilder newConfigs = new StringBuilder();

                                // 新增設定
                                if (!hasDebugConfig)
                                {
                                    newConfigs.AppendLine();
                                    newConfigs.AppendLine($"  <PropertyGroup Condition=\"'$(Configuration)|$(Platform)' == 'Debug|{platform}'\">");
                                    newConfigs.AppendLine("    <DebugSymbols>true</DebugSymbols>");
                                    newConfigs.AppendLine($"    <OutputPath>bin\\{platform}\\Debug\\</OutputPath>");
                                    newConfigs.AppendLine("    <DefineConstants>DEBUG;TRACE</DefineConstants>");
                                    newConfigs.AppendLine("    <DebugType>full</DebugType>");
                                    newConfigs.AppendLine($"    <PlatformTarget>{platform}</PlatformTarget>");
                                    newConfigs.AppendLine("    <LangVersion>7.3</LangVersion>");
                                    newConfigs.AppendLine("    <ErrorReport>prompt</ErrorReport>");
                                    newConfigs.AppendLine("  </PropertyGroup>");
                                    AppendLog($"新增 Debug|{platform} PropertyGroup 設定");
                                }

                                if (!hasReleaseConfig)
                                {
                                    newConfigs.AppendLine($"  <PropertyGroup Condition=\"'$(Configuration)|$(Platform)' == 'Release|{platform}'\">");
                                    newConfigs.AppendLine($"    <OutputPath>bin\\{platform}\\Release\\</OutputPath>");
                                    newConfigs.AppendLine("    <DefineConstants>TRACE</DefineConstants>");
                                    newConfigs.AppendLine("    <Optimize>true</Optimize>");
                                    newConfigs.AppendLine("    <DebugType>pdbonly</DebugType>");
                                    newConfigs.AppendLine($"    <PlatformTarget>{platform}</PlatformTarget>");
                                    newConfigs.AppendLine("    <LangVersion>7.3</LangVersion>");
                                    newConfigs.AppendLine("    <ErrorReport>prompt</ErrorReport>");
                                    newConfigs.AppendLine("  </PropertyGroup>");
                                    AppendLog($"新增 Release|{platform} PropertyGroup 設定");
                                }

                                if (newConfigs.Length > 0)
                                {
                                    content = content.Insert(insertPosition, newConfigs.ToString());
                                    AppendLog($"PropertyGroup 設定已新增到專案檔案（使用備用方法）");
                                }
                            }
                            else
                            {
                                AppendLog($"錯誤：無法找到插入位置");
                            }
                        }
                        else
                        {
                            AppendLog($"錯誤：找不到 ItemGroup 標籤");
                        }
                    }
                    else
                    {
                        // 取得最後一個匹配
                        Match lastMatch = matches[matches.Count - 1];
                        int insertPosition = lastMatch.Index + lastMatch.Length;

                        AppendLog($"找到 {matches.Count} 個 Condition PropertyGroup，準備在最後一個之後新增設定");

                        StringBuilder newConfigs = new StringBuilder();

                        // 如果沒有 Debug 設定，則添加
                        if (!hasDebugConfig)
                        {
                            newConfigs.AppendLine();
                            newConfigs.AppendLine($"  <PropertyGroup Condition=\"'$(Configuration)|$(Platform)' == 'Debug|{platform}'\">");
                            newConfigs.AppendLine("    <DebugSymbols>true</DebugSymbols>");
                            newConfigs.AppendLine($"    <OutputPath>bin\\{platform}\\Debug\\</OutputPath>");
                            newConfigs.AppendLine("    <DefineConstants>DEBUG;TRACE</DefineConstants>");
                            newConfigs.AppendLine("    <DebugType>full</DebugType>");
                            newConfigs.AppendLine($"    <PlatformTarget>{platform}</PlatformTarget>");
                            newConfigs.AppendLine("    <LangVersion>7.3</LangVersion>");
                            newConfigs.AppendLine("    <ErrorReport>prompt</ErrorReport>");
                            newConfigs.AppendLine("  </PropertyGroup>");
                            AppendLog($"新增 Debug|{platform} PropertyGroup 設定");
                        }

                        // 如果沒有 Release 設定，則添加
                        if (!hasReleaseConfig)
                        {
                            newConfigs.AppendLine($"  <PropertyGroup Condition=\"'$(Configuration)|$(Platform)' == 'Release|{platform}'\">");
                            newConfigs.AppendLine($"    <OutputPath>bin\\{platform}\\Release\\</OutputPath>");
                            newConfigs.AppendLine("    <DefineConstants>TRACE</DefineConstants>");
                            newConfigs.AppendLine("    <Optimize>true</Optimize>");
                            newConfigs.AppendLine("    <DebugType>pdbonly</DebugType>");
                            newConfigs.AppendLine($"    <PlatformTarget>{platform}</PlatformTarget>");
                            newConfigs.AppendLine("    <LangVersion>7.3</LangVersion>");
                            newConfigs.AppendLine("    <ErrorReport>prompt</ErrorReport>");
                            newConfigs.AppendLine("  </PropertyGroup>");
                            AppendLog($"新增 Release|{platform} PropertyGroup 設定");
                        }

                        // 插入新的設定
                        if (newConfigs.Length > 0)
                        {
                            content = content.Insert(insertPosition, newConfigs.ToString());
                            AppendLog($"PropertyGroup 設定已新增到專案檔案");
                        }
                    }
                }

                // 儲存檔案
                File.WriteAllText(projectPath, content, Encoding.UTF8);
                AppendLog($"專案檔案 {Path.GetFileName(projectPath)} 修改完成");
            }
            catch (Exception ex)
            {
                AppendLog($"修改專案檔案 {Path.GetFileName(projectPath)} 時發生錯誤: {ex.Message}");
                AppendLog($"錯誤詳情: {ex.StackTrace}");
            }
        }
        private void AppendLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AppendLog), message);
                return;
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            textBoxLog.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
            textBoxLog.ScrollToCaret();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
