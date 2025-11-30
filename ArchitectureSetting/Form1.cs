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

        private void ModifySolutionFile(string solutionFile, string platform)
        {
            try
            {
                AppendLog($"修改方案檔案: {Path.GetFileName(solutionFile)}");
                
                string content = File.ReadAllText(solutionFile, Encoding.UTF8);
                string[] lines = content.Split('\n');
                List<string> newLines = new List<string>();
                
                bool inGlobalSection = false;
                bool inSolutionConfigurationPlatforms = false;
                bool inProjectConfigurationPlatforms = false;
                List<string> projectGuids = new List<string>();
                int modifiedCount = 0;
                
                // 先找出所有專案的 GUID
                foreach (string line in lines)
                {
                    var projectMatch = Regex.Match(line, @"Project\(""{.*?}""\)\s*=\s*"".*?"",\s*"".*?"",\s*""\{(.*?)\}""");
                    if (projectMatch.Success)
                    {
                        projectGuids.Add(projectMatch.Groups[1].Value);
                    }
                }
                
                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    
                    if (trimmedLine.StartsWith("GlobalSection(SolutionConfigurationPlatforms)"))
                    {
                        inGlobalSection = true;
                        inSolutionConfigurationPlatforms = true;
                        newLines.Add(line);
                        continue;
                    }
                    
                    if (trimmedLine.StartsWith("GlobalSection(ProjectConfigurationPlatforms)"))
                    {
                        inGlobalSection = true;
                        inProjectConfigurationPlatforms = true;
                        newLines.Add(line);
                        AppendLog($"開始處理 ProjectConfigurationPlatforms 區段");
                        continue;
                    }
                    
                    if (trimmedLine == "EndGlobalSection")
                    {
                        if (inSolutionConfigurationPlatforms)
                        {
                            // 新增解決方案組態平台 (保留原功能)
                            if (!content.Contains($"Debug|{platform}"))
                            {
                                newLines.Add($"\t\tDebug|{platform} = Debug|{platform}");
                                AppendLog($"新增方案組態: Debug|{platform}");
                            }
                            if (!content.Contains($"Release|{platform}"))
                            {
                                newLines.Add($"\t\tRelease|{platform} = Release|{platform}");
                                AppendLog($"新增方案組態: Release|{platform}");
                            }
                        }
                        
                        if (inProjectConfigurationPlatforms)
                        {
                            // 為每個專案新增平台組態 (保留原功能)
                            foreach (string guid in projectGuids)
                            {
                                bool hasDebugConfig = content.Contains($"{{{guid.ToUpper()}}}.Debug|{platform}.ActiveCfg");
                                bool hasReleaseConfig = content.Contains($"{{{guid.ToUpper()}}}.Release|{platform}.ActiveCfg");
                                
                                if (!hasDebugConfig || !hasReleaseConfig)
                                {
                                    if (!hasDebugConfig)
                                    {
                                        newLines.Add($"\t\t{{{guid.ToUpper()}}}.Debug|{platform}.ActiveCfg = Debug|{platform}");
                                        newLines.Add($"\t\t{{{guid.ToUpper()}}}.Debug|{platform}.Build.0 = Debug|{platform}");
                                    }
                                    if (!hasReleaseConfig)
                                    {
                                        newLines.Add($"\t\t{{{guid.ToUpper()}}}.Release|{platform}.ActiveCfg = Release|{platform}");
                                        newLines.Add($"\t\t{{{guid.ToUpper()}}}.Release|{platform}.Build.0 = Release|{platform}");
                                    }
                                    AppendLog($"為專案 {guid} 新增 {platform} 平台組態");
                                }
                            }
                            
                            if (modifiedCount > 0)
                            {
                                AppendLog($"共修改了 {modifiedCount} 行專案組態，將平台統一改為: {platform}");
                            }
                        }
                        
                        inSolutionConfigurationPlatforms = false;
                        inProjectConfigurationPlatforms = false;
                        inGlobalSection = false;
                        newLines.Add(line);
                        continue;
                    }
                    
                    // 處理 ProjectConfigurationPlatforms 區段內的行 (新功能)
                    if (inProjectConfigurationPlatforms && !trimmedLine.StartsWith("EndGlobalSection") && !string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        // 修改等號後面的平台部分
                        string modifiedLine = ModifyProjectConfigurationLine(line, platform);
                        newLines.Add(modifiedLine);
                        
                        if (modifiedLine != line)
                        {
                            modifiedCount++;
                            AppendLog($"  修改: {line.Trim()} -> {modifiedLine.Trim()}");
                        }
                        continue;
                    }
                    
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

        private void ModifyProjectFile(string projectPath, string platform)
        {
            try
            {
                AppendLog($"修改專案檔案: {Path.GetFileName(projectPath)}");
                
                string content = File.ReadAllText(projectPath, Encoding.UTF8);
                
                // 檢查是否已經有該平台的設定
                if (content.Contains($"<Platform>{platform}</Platform>") || 
                    content.Contains($"<Platforms>") && content.Contains(platform))
                {
                    AppendLog($"專案 {Path.GetFileName(projectPath)} 已包含 {platform} 平台設定");
                    return;
                }
                
                // 尋找 <Platforms> 標籤或 <Platform> 標籤
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
                    }
                }
                else
                {
                    // 如果沒有 Platforms 標籤，在 PropertyGroup 中添加
                    string propertyGroupPattern = @"(<PropertyGroup[^>]*>)";
                    Match propertyGroupMatch = Regex.Match(content, propertyGroupPattern);
                    
                    if (propertyGroupMatch.Success)
                    {
                        string insertion = propertyGroupMatch.Groups[1].Value + Environment.NewLine + 
                                         $"    <Platforms>AnyCPU;{platform}</Platforms>";
                        content = content.Replace(propertyGroupMatch.Groups[1].Value, insertion);
                        AppendLog($"新增專案平台設定: {platform}");
                    }
                }
                
                File.WriteAllText(projectPath, content, Encoding.UTF8);
                AppendLog($"專案檔案 {Path.GetFileName(projectPath)} 修改完成");
            }
            catch (Exception ex)
            {
                AppendLog($"修改專案檔案 {Path.GetFileName(projectPath)} 時發生錯誤: {ex.Message}");
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
    }
}
