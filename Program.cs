/*

广州大学抢课脚本
GZHU CourseSelection Script
    @author: evermore(github@Weverses)
    @contributor: evermore
    @version: 0.2.10_cs
    @date: 01/03/25
    @OpenSouce LICENSE: AGPL-v3
    @EULA: 最终用户许可协议
        重要事项：
            请在使用本软件之前仔细阅读此最终用户许可协议（以下简称“协议”）。
            安装或使用本软件即表示您同意遵守本协议的条款。
            如果您不同意这些条款，请勿安装或使用本软件。
        第1条 软件授权范围
            1.1 用户可以为非商业目的在各移动端安装、使用、显示、运行、对本软件或者本软件运行过程中释放到任何终端设备内存中的数据进行复制、更改、修改、挂接运行或创作任何衍生作品。
            1.2 用户不得为商业运营目的安装、使用、运行本软件。如果需要进行商业性的销售、复制和散发，例如软件预装和捆绑，必须获得作者的授权和许可。
            1.3 允许在The GNU Affero General Public License v3.0 (AGPL-3.0) 开源协议所允许的范围内，使用由作者的源代码进行再次创作和分发。
        第2条 用户义务
            2.1 用户不得利用本软件从事任何违反法律或侵犯他人权利的行为。
            2.2 未经作者允许, 用户不得将此脚本用于盈利。
        第3条 服务风险及免责申明
            3.1. 作者不会提供服务支持。作者不保证本软件服务在操作上不会中断或没有错误，不保证会纠正本软件服务的所有缺陷，亦不保证本软件服务能满足用户的所有要求。由此产生的后果，作者不承担任何责任。
            3.2. 用户因第三方如通讯线路故障、技术问题、网络、电脑终端设备故障、系统不稳定性及其他各种不可抗力原因而遭受的一切损失，作者不承担责任。
            3.3. 作者不保证通过本软件获得的信息内容 (包括但不限于调用的第三方服务内容) 的合法性、真实性、准确性、有效性，作者不对用户做出的任何行为的结果承担任何责任。
    @updateLog: 
        0.1
            0.1.0 initial commit! Python构建
            0.1.1 [feat] 更新支持获取通识选修课程列表, 加入选择课程功能
            0.1.2 [feat] 支持查看主修课程的课表
            0.1.3 [fix]  修复课表获取逻辑, 换用新的提交课程逻辑, typo fix
            0.1.4 [feat] 支持广州大学CAS登录, 不再需要手动抓取Cookie
            0.1.5 [feat] 新增菜单逻辑, 支持返回上一级
            0.1.6 [feat] 支持多选课程, 在选择一项课程后可以继续选择, 再一并提交课程
            0.1.7 [feat] 支持删除已选课程, back返回逻辑覆盖全场景
            0.1.8 [feat] 支持抢课模式, 在进入抢课模式后, 会进入倒计时, 每30s刷新当前时间, 到达选定的时间时多线程并发提交已选择的课程
            0.1.9 [feat] 抢课模式倒计时时刷新时间频率调整为2s, 新增监听线程, 抢课模式可随时通过输入back打断回到菜单新增或删除课程
        0.2
            0.2.0 [remk] 使用C#重写整个脚本
            0.2.1 [fix]  修复大量问题, 脚本正常运行, 并换用更先进的 Playwright 来调用浏览器
            0.2.3 [fix]  typo fix, preRelease
            0.2.4 [fix]  修复抢课模式下的post请求中courseHeaders没有更改Content-Type为application/x-www-form-urlencoded导致选课失败的问题
            0.2.5 [feat] 支持自动获取各种id, 现在可以给所有人所有年份通用了
            0.2.6 [fix]  优化脚本逻辑，由于抢课阶段MainPage会非常卡，再返回到菜单再进入选课界面时不会再次获取各种id重新赋值，而是在初始化时就获取
            0.2.7 [feat] 加入缓存区，在第一次加载时，直接缓存大类课表的json，防止每次回到菜单时重新获取json的时候由于系统卡爆导致无法获取json
            0.2.8 [feat] 加入筛选，只筛选有余量人数可以进行选择的课程
            0.2.9 [feat] 由于选课前没有提前开放系统，在进入脚本后可以选择[预先加载课程列表]和[加载 预先已选择的课程列表]功能
            0.2.10[fix]  修复提交选课后的返回结果课程名称为未知的问题, 加入选课失败原因说明
*/

/*

*/
using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using Microsoft.Playwright;
using System.Data;

namespace GZHU_CourseSelection_CSharp
{
    class Program
    {
        private static string Readme = @"
    @author: evermore(github@Weverses)
    @contributor: evermore
    @version: 0.2.10_cs
    @date: 01/03/25
    @OpenSouce LICENSE: AGPL-v3
    @EULA: 最终用户许可协议
        重要事项：
            请在使用本软件之前仔细阅读此最终用户许可协议（以下简称“协议”）。
            安装或使用本软件即表示您同意遵守本协议的条款。
            如果您不同意这些条款，请勿安装或使用本软件。
        第1条 软件授权范围
            1.1 用户可以为非商业目的在各移动端安装、使用、显示、运行、对本软件或者本软件运行过程中释放到任何终端设备内存中的数据进行复制、更改、修改、挂接运行或创作任何衍生作品。
            1.2 用户不得为商业运营目的安装、使用、运行本软件。如果需要进行商业性的销售、复制和散发，例如软件预装和捆绑，必须获得作者的授权和许可。
            1.3 允许在The GNU Affero General Public License v3.0 (AGPL-3.0) 开源协议所允许的范围内，使用由作者的源代码进行再次创作和分发。
        第2条 用户义务
            2.1 用户不得利用本软件从事任何违反法律或侵犯他人权利的行为。
            2.2 未经作者允许, 用户不得将此脚本用于盈利。
        第3条 服务风险及免责申明
            3.1. 作者不会提供服务支持。作者不保证本软件服务在操作上不会中断或没有错误，不保证会纠正本软件服务的所有缺陷，亦不保证本软件服务能满足用户的所有要求。由此产生的后果，作者不承担任何责任。
            3.2. 用户因第三方如通讯线路故障、技术问题、网络、电脑终端设备故障、系统不稳定性及其他各种不可抗力原因而遭受的一切损失，作者不承担责任。
            3.3. 作者不保证通过本软件获得的信息内容 (包括但不限于调用的第三方服务内容) 的合法性、真实性、准确性、有效性，作者不对用户做出的任何行为的结果承担任何责任。";
        private static bool Debug = false;

        // 通用Headers
        private static Dictionary<string, string> courseHeaders = new Dictionary<string, string>()
        {
            { "Cookie", "" },
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36" }
        };

        // 主页URL
        private static string mainPageURL = "https://jwxt.gzhu.edu.cn/jwglxt/xsxk/zzxkjrb_cxZzxkJrbIndex.html?gnmkdm=N253522&layout=default&gnmkdm=N253522&layout=default&hasHeader=true";
        private static Dictionary<string, string> mainPageData = new Dictionary<string, string>();

        // 获取所有课程列表的URL和数据
        private static string allCourseURL = "https://jwxt.gzhu.edu.cn/jwglxt/xsxk/zzxkjrb_cxZzxkJrbDisplay.html?gnmkdm=N253522";
        private static Dictionary<string, string> allCourseData = new Dictionary<string, string>()
        {
            {"yl_list[0]", "1"},
            { "rwlx", "1" },
            { "xkly", "0" },
            { "bklx_id", "0" },
            { "sfkkjyxdxnxq", "0" },
            { "xqh_id", "1" },
            { "sfkknj", "0" },
            { "gnjkxdnj", "0" },
            { "sfkkzy", "0" },
            { "kzybkxy", "0" },
            { "sfkcfx", "0" },
            { "xkkz_id", "" },
            { "sfznkx", "0" },
            { "zdkxms", "0" },
            { "sfkxq", "0" },
            { "kkbk", "0" },
            { "kkbkdj", "0" },
            { "njdm_id", "" },
            { "jg_id", "" },
            { "zyh_id", "" },
            { "doWhat", "card" },
            { "sfkgbcx", "0" },
            { "sfrxtgkcxd", "0" },
            { "tykczgxdcs", "0" },
            { "zyfx_id", "" },
            { "bh_id", "" },
            { "zh", "" },
            { "xbm", "1" },
            { "xslbdm", "" },
            { "mzm", "01" },
            { "xz", "" },
            { "ccdm", "1" },
            { "xsbj", "" },
            { "xkxnm", "" },
            { "xkxqm", "" },
            { "kklxdm", "01" },
            { "kspage", "0" },
            { "jspage", "90" }
        };

        // 获取子课程详细信息的URL
        private static string subCourseURL = "https://jwxt.gzhu.edu.cn/jwglxt/xsxk/zzxkjrb_cxJxbWithKchZzxkJrb.html?gnmkdm=N253522";

        // 主修课程数据
        private static Dictionary<string, string> subMajorCourseData = new Dictionary<string, string>()
        {
            { "rwlx","1" }, { "bklx_id","0" }, { "zh","" }, { "xqh_id","" },
            { "xkkz_id","" }, { "jg_id","" }, { "zyh_id","" }, { "zyfx_id","" },
            { "njdm_id","" }, { "bh_id","" },
            { "xbm","1" }, { "xslbdm","" }, { "mzm","01" }, { "xz","" }, { "ccdm","1" },
            { "xkly","2" }, { "xsbj","" }, { "sfkknj","0" }, { "gnjkxdnj","0" },
            { "sfkkzy","0" }, { "kzybkxy","0" }, { "sfznkx","0" }, { "zdkxms","0" },
            { "sfkxq","0" }, { "kkbk","0" }, { "kkbkdj","0" }, { "xkxnm","" },
            { "xkxqm","" }, { "kklxdm","01" }, { "kch_id","" }
        };

        // 通识选修课程数据
        private static Dictionary<string, string> subOpDataCourse = new Dictionary<string, string>()
        {
            { "rwlx","2"}, { "bklx_id","0"}, { "zh",""}, { "xqh_id",""},
            { "xkkz_id",""}, { "jg_id",""},
            { "zyh_id",""}, { "zyfx_id",""}, { "njdm_id",""},
            { "bh_id",""},
            { "xbm",""}, { "xslbdm",""}, { "mzm","01"}, { "xz","" },
            { "ccdm","1"}, { "xkly","0"}, { "xsbj","" },
            { "sfkknj","0"}, { "gnjkxdnj","0"}, { "sfkkzy","0"}, { "kzybkxy","0" },
            { "sfznkx","0"}, { "zdkxms","0" }, { "sfkxq","0"}, { "kkbk","0" },
            { "kkbkdj","0" }, { "xkxnm",""}, { "xkxqm",""}, { "kklxdm","10" },
            { "kch_id","" }
        };

        // 选课提交的URL和数据
        private static string postCourseURL = "https://jwxt.gzhu.edu.cn/jwglxt/xsxk/zzxkjrb_xkBcZyZzxkJrb.html?gnmkdm=N253522";
        private static Dictionary<string, string> postCourseData = new Dictionary<string, string>()
        {
            { "jxb_ids", "" },
            { "kch_id", "" },
            { "xxkbj", "0" },
            { "qz", "0" },
            { "cxbj", "0" },
            { "xkkz_id", "" },
            { "njdm_id", "2024" },
            { "zyh_id", "" },
            { "xkxnm", "2024" },
            { "xkxqm", "12" }
        };

        // 动态赋值
        private static string zyh_id = "";
        private static string major_xkkz_id = "";
        private static string optional_xkkz_id = "";
        private static string courseType = "";  // "1" 或 "2"
        private static string xkkz_id = "";
        private static string courseName = "";

        private static JArray cachedMajorJson = null;
        private static JArray cachedOptionalJson = null;

        private static Dictionary<string, string> hiddenInputDictCache = new Dictionary<string, string>();

        private static string username = "";
        // HttpClient 复用
        private static readonly HttpClient httpClient = new HttpClient();

        // ----------------------- Main入口 -----------------------
        static async Task Main(string[] args)
        {
            Console.WriteLine("");
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("         欢迎进入广州大学抢课脚本");
            Console.WriteLine("        GZHU CourseSelection Script");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine($"{Readme}");
            Console.ResetColor();
            Console.WriteLine("----------------------------------------------");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("[TIPS] 使用该脚本即已代表同意脚本中的EULA最终用户许可协议");
            Console.ResetColor();
            Console.WriteLine("[INFO] 开始初始化");
            // await EnsurePlaywrightBrowsersInstalled();
            Console.WriteLine("[INFO] 开始获取 Cookie");
            Console.WriteLine("[TIPS] 本脚本不会以任何方式将您的账号密码信息保存并上传到云端, 所有数据均留在本地进行处理");

            // 登录
            string login_url = "https://jwxt.gzhu.edu.cn/sso/driot4login";
            while (true)
            {
                var (username, password) = PromptCredentials();

                Console.WriteLine("[INFO] 正在启动浏览器并进行自动化登录...");
                string cookie = await GetCookieWithCASLogin(login_url, username, password);
                if (!string.IsNullOrEmpty(cookie))
                {
                    // 校验 Cookie
                    if (IsValidCookie(cookie))
                    {
                        Console.WriteLine("[INFO] Cookie 已成功加载");
                        courseHeaders["Cookie"] = cookie;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("[ERROR] Cookie 格式错误, 请联系作者!!");
                        Environment.Exit(1);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] 登录失败，请重新输入账号密码。\n");
                    Console.ResetColor();
                }
            }

            // 新增：选择是否预先加载或加载本地记录
            Console.WriteLine("\n-------------------------------------");
            Console.WriteLine("[LOADING] 请选择脚本操作");
            Console.WriteLine("a. 预先加载课程列表");
            Console.WriteLine("b. 加载 预先选择的课程列表 记录");
            Console.WriteLine("c. 什么都不做，继续脚本");

            string preChoice = Console.ReadLine().Trim().ToLower();

            // 初始化选课列表
            Dictionary<string, List<Dictionary<string, object>>> selectedCourses = new Dictionary<string, List<Dictionary<string, object>>>();

            // [MODIFY START] 新增：处理预先加载或加载本地记录
            if (preChoice == "b")
            {
                // 尝试加载本地记录
                LoadLocalData(username, selectedCourses);
            }
            else if (preChoice == "a")
            {
                
            }
            // [MODIFY END]


            // 继续进行 GetMainPageData，若 hiddenInputDictCache 为空，才执行
            if (hiddenInputDictCache.Count == 0)
            {
                GetMainPageData();
            }
            else
            {
                // 将缓存的 hiddenInputDict 写入
                UpdateCourseDataDictionaries(hiddenInputDictCache);
                Console.WriteLine("[INFO] 已加载本地 hiddenInputDict，无需再次解析.");
            }

            // 初始化各项id
            GetMainPageData();

            // 主菜单循环
            while (true)
            {
                Console.WriteLine("\n-------------------------------------");
                Console.WriteLine("请选择下一步操作:");
                Console.WriteLine("1. 添加课程到选课列表");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("2. 提交选课请求");
                Console.ResetColor();
                Console.WriteLine("3. 查看已选择的课程");
                Console.WriteLine("4. 删除已选择的课程");
                Console.WriteLine("5. 退出脚本");
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("6. 抢课模式");
                // [MODIFY START] 新增“保存到本地”选项
                if (preChoice == "a") 
                {
                    Console.WriteLine("7. 保存这些已选择的课程列表数据到本地");
                }
                // [MODIFY END]
                Console.ResetColor();
                if (preChoice == "a") {
                    Console.Write("请输入你的选择 (1/2/3/4/5/6/7): ");
                } else {
                    Console.Write("请输入你的选择 (1/2/3/4/5/6): ");
                }

                string choice = Console.ReadLine().Trim();

                switch (choice)
                {
                    case "1":
                        // 添加课程到选课列表
                        bool? added = PerformCourseSelection(selectedCourses);
                        if (added == true)
                        {
                            // Console.ForegroundColor = ConsoleColor.DarkBlue;
                            // Console.WriteLine("[SUCCESS] 课程已添加到选课列表。");
                            // Console.ResetColor();
                        }
                        break;
                    case "2":
                        // 提交选课请求
                        if (selectedCourses.Count == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[ERROR] 选课列表为空，请先添加课程。");
                            Console.ResetColor();
                            continue;
                        }
                        SubmitSelectedCourses(selectedCourses);
                        break;
                    case "3":
                        // 查看已选择的课程
                        if (selectedCourses.Count == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[INFO] 当前选课列表为空。");
                            Console.ResetColor();
                        }
                        else
                        {
                            ShowSelectedCourses(selectedCourses);
                        }
                        break;
                    case "4":
                        // 删除已选择的课程
                        DeleteSelectedCourse(selectedCourses);
                        break;
                    case "5":
                        // 退出脚本
                        Console.WriteLine("[INFO] 退出脚本。感谢使用！");
                        Environment.Exit(0);
                        break;
                    case "6":
                        // 抢课模式
                        SnatchCourseMode(selectedCourses);
                        break;
                    // [MODIFY START] 新增选项7: 保存本地
                    case "7":
                        if (preChoice == "a")
                        {
                            SaveLocalData(username, selectedCourses);
                        }
                        else
                        {
                            Console.WriteLine("[ERROR] 当前不支持保存到本地, 因为未选择预先加载模式!");
                        }
                        break;
                    // [MODIFY END]
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ERROR] 输入无效，请输入 1、2、3、4、5 或 6。\n");
                        Console.ResetColor();
                        break;
                }
            }
        }

        // prompt_credentials
        static (string, string) PromptCredentials()
        {
            // Python 中会先读环境变量, 这里示例仅简单在控制台读取
            Console.Write("[USERNAME] 请输入你的数字广大学号: ");
            username = Console.ReadLine().Trim();

            Console.Write("[PASSWORD] 请输入你的数字广大密码: ");
            // 若想不回显，可以更换获取密码的方法（如使用 SecureString 或第三方库）
            string password = ReadPassword();
            return (username, password);
        }

        // 读取密码，不回显
        static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key != ConsoleKey.Backspace && keyInfo.Key != ConsoleKey.Enter)
                {
                    password += keyInfo.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password.Substring(0, password.Length - 1);
                        int cursorPos = Console.CursorLeft;
                        Console.SetCursorPosition(cursorPos - 1, Console.CursorTop);
                        Console.Write(" ");
                        Console.SetCursorPosition(cursorPos - 1, Console.CursorTop);
                    }
                }
            }
            while (keyInfo.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return password;
        }

        // isValidCookie
        static bool IsValidCookie(string cookieStr)
        {
            // 定义正则表达式模式
            string jsessionid_pattern = @"JSESSIONID=[^;]+";
            string sf_cookie_pattern = @"SF_cookie_[^=]+=[^;]+";

            bool has_jsessionid = Regex.IsMatch(cookieStr, jsessionid_pattern);
            bool has_sf_cookie = Regex.IsMatch(cookieStr, sf_cookie_pattern);

            return has_jsessionid && has_sf_cookie;
        }

        // GetCookieWithCASLogin (对应 getCookieWithCASIogin)
        // 使用 Playwright 进行自动化登录并获取 Cookie
        static async Task<string> GetCookieWithCASLogin(string loginUrl, string username, string password)
        {
            string cookieStr = "";
            try
            {
                using var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true, // 设置为 true 启用无头模式
                Args = new[] { "--start-maximized" }
            });

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 540, Height = 280 }
            });

            var page = await context.NewPageAsync();
            Console.WriteLine("[INFO] 浏览器已启动，开始导航到登录页面...");

            await page.GotoAsync(loginUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            Console.WriteLine("[INFO] 浏览器已运行, 正在填充账号密码...");

            await page.FillAsync("#un", username); // 替换为实际用户名输入框的选择器
            await page.FillAsync("#pd", password); // 替换为实际密码输入框的选择器

            // 点击登录按钮
            await page.ClickAsync("#index_login_btn"); // 替换为实际登录按钮的选择器

            Console.WriteLine("[INFO] 已提交登录表单，等待登录完成...");

            try
            {
                // 登录成功后会有元素 #xtmc 出现
                await page.WaitForSelectorAsync("#xtmc", new PageWaitForSelectorOptions
                {
                    Timeout = 30000
                });
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[SUCCESS] 登录成功！");
                Console.ResetColor();
            }
            catch (TimeoutException)
            {
                // 检查是否登录失败
                bool loginFailed = await page.QuerySelectorAsync("#index_login_btn") != null;
                if (loginFailed)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] 账号或密码错误!");
                    Console.ResetColor();
                    await browser.CloseAsync();
                    return null;
                }

                // 检查是否需要手动验证
                bool accessDenied = await page.QuerySelectorAsync(".authorise_title") != null;
                if (accessDenied)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] 访问被拒绝, 账号或密码错误次数超过5次! 请重新输入正确的账号密码");
                    Console.ResetColor();
                    await browser.CloseAsync();
                    return null;
                }

                // 需要手动验证
                Console.WriteLine("[INFO] 检测到可能需要手动验证。请在浏览器中完成任何额外的登录步骤（如验证码、双因素认证等），然后按 Enter 键继续...");
                Console.ReadLine();

                try
                {
                    await page.WaitForSelectorAsync("#xtmc", new PageWaitForSelectorOptions
                    {
                        Timeout = 30000
                    });
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.WriteLine("[INFO] 登录成功！");
                    Console.ResetColor();
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("[ERROR] 登录过程仍然超时, 登录失败, 请检查网络 ");
                    await browser.CloseAsync();
                    return null;
                }
            }

            // 获取 Cookie
            var cookies = await context.CookiesAsync();
        
            // 打印所有 Cookie
            if (Debug) {
                Console.WriteLine($"[DEBUG] 获取到 {cookies.Count} 个 Cookie。");
                Console.WriteLine("[DEBUG] 获取到的所有 Cookie:");
                foreach (var ck in cookies)
                {
                    Console.WriteLine($"- 名称: {ck.Name}, 值: {ck.Value}");
                }
            }
        

            // 筛选第一个 JSESSIONID
            var jsessionCookies = cookies.Where(ck => ck.Name == "JSESSIONID").ToList();
            var lastJsessionCookie = jsessionCookies.FirstOrDefault();

            // 筛选 SF_cookie_18
            var sfCookie = cookies.FirstOrDefault(ck => ck.Name == "SF_cookie_18");

            // 检查是否找到必要的 Cookie
            if (lastJsessionCookie == null || sfCookie == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] 未找到必要的 Cookie (JSESSIONID 或 SF_cookie_18)。");
                Console.ResetColor();
                await browser.CloseAsync();
                return null;
            }

            // 打印筛选后的 Cookie
            if (Debug) {
                Console.WriteLine("[DEBUG] 筛选后的 Cookie:");
                Console.WriteLine($"- 名称: {lastJsessionCookie.Name}, 值: {lastJsessionCookie.Value}");
                Console.WriteLine($"- 名称: {sfCookie.Name}, 值: {sfCookie.Value}");
            }
        
            // 构建 Cookie 字符串
            var cookieList = new List<string>
            {
                $"{lastJsessionCookie.Name}={lastJsessionCookie.Value}",
                $"{sfCookie.Name}={sfCookie.Value}"
            };
            cookieStr = string.Join("; ", cookieList);

            if (Debug) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[SUCCESS] 成功获取并筛选登录后的 Cookie");
                Console.ResetColor();
            }
        
            await browser.CloseAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] 获取 Cookie 过程中出错: {ex}");
            cookieStr = "";
        }
        if (Debug) {
            Console.WriteLine($"[SUCCESS] 成功获取并筛选登录后的 Cookie: {cookieStr}");
        }
        return cookieStr;
    }
        
    static async Task EnsurePlaywrightBrowsersInstalled() {
    // 检查 Playwright 浏览器是否已安装
    string playwrightBrowsersPath = Path.Combine(Environment.CurrentDirectory, ".playwright");
    if (!Directory.Exists(playwrightBrowsersPath) || !Directory.EnumerateFiles(playwrightBrowsersPath).Any())
    {
        Console.WriteLine("[INFO] Playwright 浏览器未安装，正在安装...");
        try
        {
            // 运行 playwright install 命令
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "pwsh",
                Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"playwright.ps1 install\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                string stdout = await process.StandardOutput.ReadToEndAsync();
                string stderr = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("[SUCCESS] Playwright 浏览器安装成功。");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERROR] Playwright 浏览器安装失败:\n{stderr}");
                    Console.ResetColor();
                    Environment.Exit(1);
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] 自动安装 Playwright 浏览器时出错: {ex.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }
    }
    else
    {
        Console.WriteLine("[INFO] Playwright 浏览器已安装。");
    }
}
        // PerformCourseSelection (对应 perform_course_selection)
        static bool? PerformCourseSelection(Dictionary<string, List<Dictionary<string, object>>> selectedCourses)
        {
            GetAllCourseList(selectedCourses);
            // 这里返回 null or bool仅为让主菜单知道是否添加了课程
            return true;
        }

        // getAllCourseList
        static void GetAllCourseList(Dictionary<string, List<Dictionary<string, object>>> selectedCourses)
        {
            if (string.IsNullOrEmpty(major_xkkz_id) || string.IsNullOrEmpty(optional_xkkz_id))
            {
                Console.WriteLine("\u001b[31m[ERROR] 解析课程分类信息失败，请检查正则或网页结构。\u001b[0m");
                Environment.Exit(1);
            }

            while (true)
            {
                Console.WriteLine("-------------------------------------");
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("[INFO] 请选择你要选的课程大类: \n[1] 主修课程\n[2] 通识选修");
                Console.ResetColor();
                Console.Write("[INPUT] 请输入: 序号(比如1) (或输入 'back' 返回上一级菜单): ");
                string input = Console.ReadLine().Trim().ToLower();
                if (input == "back") return;

                if (input == "1")
                {
                    courseType = "1";
                    allCourseData["rwlx"] = "1";
                    xkkz_id = major_xkkz_id;
                    // 如果缓存为空，则发起请求；如果缓存不为空，直接使用缓存
                    if (cachedMajorJson == null)
                    {
                        Console.WriteLine($"[STEP] 开始获取主修课程列表");
                        allCourseData["xkkz_id"] = xkkz_id;
                        var resp = new PostClass(allCourseURL, allCourseData, courseHeaders);
                        var respJson = resp.GetList();

                        // 检查 respJson 是否为 JArray
                        if (respJson == null || !(respJson is JArray))
                        {
                            Console.WriteLine("\u001b[31m[ERROR] 获取主修课程列表时响应格式不正确。\u001b[0m");
                            Environment.Exit(1);
                        }
                        cachedMajorJson = (JArray)respJson;
                        GetSubCourseList(cachedMajorJson, selectedCourses, "主修课程");
                    }
                    else
                    {
                        Console.WriteLine("[INFO] 检测到已缓存的主修课程列表，直接使用缓存。");
                        GetSubCourseList(cachedMajorJson, selectedCourses, "主修课程");
                    }
                    break;
                }
                else if (input == "2")
                {
                    courseType = "2";
                    allCourseData["rwlx"] = "2";
                    xkkz_id = optional_xkkz_id;

                    if (cachedOptionalJson == null)
                    {
                        Console.WriteLine($"[STEP] 开始获取通识选修课程列表");
                        allCourseData["xkkz_id"] = xkkz_id;
                        var resp = new PostClass(allCourseURL, allCourseData, courseHeaders);
                        var respJson = resp.GetList();

                        if (respJson == null || !(respJson is JArray))
                        {
                            Console.WriteLine("\u001b[31m[ERROR] 获取通识选修课程列表时响应格式不正确。\u001b[0m");
                            Environment.Exit(1);
                        }
                        cachedOptionalJson = (JArray)respJson;
                        GetSubCourseList(cachedOptionalJson, selectedCourses, "通识选修");
                    }
                    else
                    {
                        Console.WriteLine("[INFO] 检测到已缓存的通识选修课程列表，直接使用缓存。");
                        GetSubCourseList(cachedOptionalJson, selectedCourses, "通识选修");
                    }
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] 输入有误, 请重试");
                    Console.ResetColor();
                }
            }
        }
        // [MODIFY START] 新增：保存到本地
        private static void SaveLocalData(string username, Dictionary<string, List<Dictionary<string, object>>> selectedCourses)
        {
            try
            {
                // database 文件夹
                string dbDir = Path.Combine(Environment.CurrentDirectory, "database");
                if (!Directory.Exists(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                }

                // 文件名：用户名
                string filePath = Path.Combine(dbDir, username + ".json");

                // 需要保存：selectedCourses + hiddenInputDictCache
                var dataToSave = new
                {
                    selected_courses = selectedCourses,
                    hidden_input_dict = hiddenInputDictCache
                };

                string jsonStr = JsonConvert.SerializeObject(dataToSave, Formatting.Indented);
                File.WriteAllText(filePath, jsonStr);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[SUCCESS] 已成功保存到本地: " + filePath);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] 保存本地记录时出错: " + ex.Message);
                Console.ResetColor();
            }
        }

        // [MODIFY START] 新增：从本地加载
        private static void LoadLocalData(string username, Dictionary<string, List<Dictionary<string, object>>> selectedCourses)
        {
            try
            {
                string dbDir = Path.Combine(Environment.CurrentDirectory, "database");
                if (!Directory.Exists(dbDir))
                {
                    Console.WriteLine("[INFO] 数据库目录不存在，无法加载本地记录。");
                    return;
                }

                string filePath = Path.Combine(dbDir, username + ".json");
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("[INFO] 未找到当前用户的本地记录文件。");
                    return;
                }

                string jsonStr = File.ReadAllText(filePath);
                var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);
                if (jsonObj == null || !jsonObj.ContainsKey("selected_courses") || !jsonObj.ContainsKey("hidden_input_dict"))
                {
                    Console.WriteLine("[INFO] 本地记录文件格式不正确或为空。");
                    return;
                }

                // 读取 selected_courses
                var selectedCoursesObj = jsonObj["selected_courses"];
                if (selectedCoursesObj != null)
                {
                    // 强转回 Dictionary<string, List<Dictionary<string, object>>>
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(selectedCoursesObj.ToString());
                    if (dict != null && dict.Count > 0)
                    {
                        // 覆盖当前空列表
                        foreach (var kv in dict)
                        {
                            selectedCourses[kv.Key] = kv.Value;
                        }
                        Console.WriteLine("[INFO] 已成功加载本地的已选择课程列表记录!");
                    }
                }

                // 读取 hidden_input_dict
                var hiddenDictObj = jsonObj["hidden_input_dict"];
                if (hiddenDictObj != null)
                {
                    var hiddenDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(hiddenDictObj.ToString());
                    if (hiddenDict != null && hiddenDict.Count > 0)
                    {
                        hiddenInputDictCache = hiddenDict;
                        Console.WriteLine("[INFO] 已成功加载本地的 hiddenInputDict!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] 加载本地记录时出错: " + ex.Message);
            }
        }
        // [MODIFY END]

        static void ShowSelectedCourses(Dictionary<string, List<Dictionary<string, object>>> selectedCourses)
        {
            Console.WriteLine("\n[INFO] 已选择的课程列表:");
            Console.WriteLine("-------------------------------------");
            foreach (var kv in selectedCourses)
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine($"类别: {kv.Key}");
                foreach (var c in kv.Value)
                {
                    Console.WriteLine($"  课程ID: {c["jxb_id"]}");
                    Console.WriteLine($"  课程名称: {c.GetValueOrDefault("course_name", "未知")}");
                    Console.WriteLine($"  教师: {c["course_teacher"]}");
                    Console.WriteLine($"  总人数: {c["course_allnum"]}");
                    Console.WriteLine($"  已选人数: {c["course_senum"]}");
                    Console.WriteLine($"  时间: {c["course_time"]}");
                    Console.WriteLine($"  教室: {c["course_room"]}");
                    Console.ResetColor();
                }
            }
        }

        // getMainPageData
        static void GetMainPageData()
        {
            var getMainPage = new PostClass(mainPageURL, mainPageData, courseHeaders);
            string respHtml = getMainPage.GetHTML();

            // 使用 HtmlAgilityPack 解析 HTML
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(respHtml);

            // 使用 XPath 选择器定位到目标 <a> 标签
            // 目标 <a> 标签具有属性 data-kklxmc 和 onclick
            var courseLinks = htmlDoc.DocumentNode.SelectNodes("//a[@data-kklxmc and @onclick]");

            if (courseLinks == null || courseLinks.Count == 0)
            {
                Console.WriteLine("\u001b[31m[ERROR] 未找到任何匹配的课程信息。\u001b[0m");
                Console.WriteLine("----- 开始输出 respHtml -----");
                Console.WriteLine(respHtml);
                Console.WriteLine("----- 结束输出 respHtml -----");
                return;
            }

            major_xkkz_id = "";
            optional_xkkz_id = "";
            zyh_id = "";
            string njdm_id_local = "";
            string xkxnm = "";

            foreach (var link in courseLinks)
            {
                // 提取 data-kklxmc 属性
                string courseName_ = link.GetAttributeValue("data-kklxmc", "未知类型");

                // 提取 onclick 属性，并解析其中的参数
                string onclickValue = link.GetAttributeValue("onclick", "");

                if (Debug)
                {
                    Console.WriteLine($"[DEBUG] Found course category: {courseName_}");
                    Console.WriteLine($"[DEBUG] onclick attribute: {onclickValue}");
                }

                // 修正后的正则匹配
                var onclickMatch = Regex.Match(onclickValue, @"queryCourse\('([^']+)','([^']+)','([^']+)','([^']+)'\)");
                if (onclickMatch.Success && onclickMatch.Groups.Count >= 5)
                {
                    string kklxdm = onclickMatch.Groups[1].Value;
                    string xkkz_id_val = onclickMatch.Groups[2].Value;
                    string njdm_id = onclickMatch.Groups[3].Value;
                    string zyh_id_val = onclickMatch.Groups[4].Value;

                    if (Debug)
                    {
                        Console.WriteLine($"[DEBUG] Parsed kklxdm: {kklxdm}, xkkz_id: {xkkz_id_val}, njdm_id: {njdm_id}, zyh_id: {zyh_id_val}");
                    }

                    if (courseName_ == "主修课程")
                        major_xkkz_id = xkkz_id_val;
                    else if (courseName_ == "通识选修")
                        optional_xkkz_id = xkkz_id_val;

                    if (string.IsNullOrEmpty(zyh_id))
                        zyh_id = zyh_id_val;
                    if (string.IsNullOrEmpty(njdm_id_local))
                        njdm_id_local = njdm_id;
                        xkxnm = njdm_id;
                }
                else
                {
                    if (Debug)
                    {
                        Console.WriteLine("\u001b[31m[DEBUG] 正则匹配失败。\u001b[0m");
                    }
                }
                try {
                // 选择所有 <input type="hidden"> 元素
                var hiddenInputs = htmlDoc.DocumentNode.SelectNodes("//input[@type='hidden']");

                if (hiddenInputs == null || hiddenInputs.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] 未找到任何隐藏的input元素。");
                    Console.ResetColor();
                    return;
                }

                // 存储id、name和value的字典
                Dictionary<string, string> hiddenInputDict = new Dictionary<string, string>();

                foreach (var input in hiddenInputs)
                {
                    string id = input.GetAttributeValue("id", "");
                    string name = input.GetAttributeValue("name", "");
                    string value = input.GetAttributeValue("value", "");

                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name))
                    {
                        hiddenInputDict[name] = value;
                        if (Debug)
                        {
                            Console.WriteLine($"[DEBUG] Found hidden input - id: {id}, name: {name}, value: {value}");
                        }
                    }
                }

                // 传入参数
                hiddenInputDictCache = hiddenInputDict;
                // 更新 allCourseData, subMajorCourseData, subOpDataCourse, postCourseData
                UpdateCourseDataDictionaries(hiddenInputDict);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[SUCCESS] 成功获取并更新隐藏的input元素。");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] 获取或解析隐藏的input元素时出错: {ex.Message}");
                Console.ResetColor();
            }
            }

            // 批量更新
            Dictionary<string, string> updates = new Dictionary<string, string>(){
                { "zyh_id", zyh_id },
                { "njdm_id", njdm_id_local },
                { "xkxnm", xkxnm}
            };
            var dicts = new List<Dictionary<string, string>>() { allCourseData, subMajorCourseData, subOpDataCourse, postCourseData };
            foreach (var d in dicts)
            {
                foreach (var u in updates)
                {
                    d[u.Key] = u.Value;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[SUCCESS] 成功解析并获取课程分类信息。");
            Console.ResetColor();
        }

        /// <summary>
        /// 更新 allCourseData, subMajorCourseData, subOpDataCourse, postCourseData 字典中的对应键值。
        /// </summary>
        /// <param name="hiddenInputs">包含 name 和 value 的字典。</param>
        static void UpdateCourseDataDictionaries(Dictionary<string, string> hiddenInputs)
        {
            // 定义需要更新的字典列表
            var dictsToUpdate = new List<Dictionary<string, string>>()
            {
                allCourseData,
                subMajorCourseData,
                subOpDataCourse,
                postCourseData
            };

            foreach (var dict in dictsToUpdate)
            {
                foreach (var kv in hiddenInputs)
                {
                    if (!string.IsNullOrEmpty(kv.Value) && dict.ContainsKey(kv.Key))
                    {
                        dict[kv.Key] = kv.Value;
                        if (Debug)
                        {
                            Console.WriteLine($"[DEBUG] Updated '{kv.Key}' to '{kv.Value}' in dictionary.");
                        }
                    }
                }
            }
        }
        // getSubCourseList
        static void GetSubCourseList(JArray respJson, Dictionary<string, List<Dictionary<string, object>>> selectedCourses, string category)
        {
            if (respJson.Count < 2)
            {
                Console.WriteLine("\u001b[31m[ERROR] 响应JSON数组元素不足。\u001b[0m");
                Environment.Exit(1);
            }

            // 提取课程类型
            string kklxmc = respJson[0].GetValueOrDefault("kklxmc", "未知类型");
            Console.WriteLine($"[INFO] 获取到课程类型: {kklxmc}");

            // 提取该课程类型下的课程列表
            var courses = respJson[1] as JArray;

            if (courses == null)
            {
                Console.WriteLine("\u001b[31m[ERROR] 课程列表的结构不正确。请联系作者反馈\033[0m");
                Environment.Exit(1);
            }

            // 输出
            Console.WriteLine("[INFO] 获取到可选课程:");
            Console.WriteLine("-------------------------------------");
            foreach (var course in courses)
            {
                string kch_id = course["kch_id"]?.ToString() ?? "";
                string kcmc = course["kcmc"]?.ToString() ?? "";
                Console.WriteLine($"\u001b[32m- id={kch_id}, {kcmc}\u001b[0m");
            }

            Console.WriteLine("-------------------------------------");

            // 用户输入课程ID并获取详细信息
            while (true)
            {
                Console.Write("[STEP] 请输入课程id, 查看该类别下的课程详情 (或输入 'back' 返回主菜单): \n");
                string courseID = Console.ReadLine().Trim();
                if (courseID.ToLower()== "back")
                {
                    return;  // 返回主菜单
                }

                // 寻找匹配的课程
                var matched_course = courses.FirstOrDefault(c => c["kch_id"]?.ToString() == courseID);
                if (matched_course != null)
                {
                    string courseNameLocal = matched_course["kcmc"]?.ToString() ?? "未知课程名称";
                    Console.WriteLine($"[INFO] 找到课程 '{courseNameLocal}'");
                    Console.WriteLine($"[INFO] 开始获取课程ID: {courseID} 的详细信息...");
                    // 调用 getCourseList
                    GetCourseList(courseID, courseNameLocal, selectedCourses, category);
                    break;
                }
                else
                {
                    Console.WriteLine("\u001b[31m[ERROR] 未找到对应的课程ID。请确认输入是否正确或输入 'back' 返回上一级菜单。\u001b[0m");
                    // 继续循环以允许重新输入或 'back'
                }
            }
        }

        // getCourseList
        static void GetCourseList(string courseID, string courseNameLocal, Dictionary<string, List<Dictionary<string, object>>> selectedCourses, string category)
        {
            string URL = subCourseURL;
            Dictionary<string, string> HEADERS = courseHeaders;

            Dictionary<string, string> DATA;
            if (courseType == "1")
            {
                DATA = new Dictionary<string, string>(subMajorCourseData);
            }
            else
            {
                DATA = new Dictionary<string, string>(subOpDataCourse);
            }

            // 修改字典中的kch_id
            DATA["kch_id"] = courseID;
            DATA["xkkz_id"] = xkkz_id;

            // 添加调试信息：显示发送的 POST 数据
            if (Debug){
                Console.WriteLine($"\n[DEBUG] Sending POST to {URL} with xkkz_id={DATA["xkkz_id"]}, kch_id={DATA["kch_id"]}");
            }
            
            var resp = new PostClass(URL, DATA, HEADERS);
            var respJson = resp.GetList();

            // 检查 respJson 是否为 JArray
            if (respJson == null || !(respJson is JArray))
            {
                Console.WriteLine("\u001b[31m[ERROR] 获取详细课程信息时响应格式不正确。\u001b[0m");
                Environment.Exit(1);
            }

            // 使用字典存储课程信息及唯一ID，并设置为不区分大小写
            Dictionary<string, Dictionary<string, object>> idMapping = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

            var courses = (JArray)respJson;
            int idx = 1;
            foreach (var course in courses)
            {
                // 提取需要的字段
                string kch_id = course["kch_id"]?.ToString() ?? "未知课程kch-ID";
                string jxb_id = course["jxb_id"]?.ToString() ?? "未知课程jxb-ID";
                string do_jxb_id = course["do_jxb_id"]?.ToString() ?? "未知课程jxb-IDs";
                string jsxx = course["jsxx"]?.ToString() ?? "未知教师";
                string jxbrl = course["jxbrl"]?.ToString() ?? "0";
                string yxzrs = course["yxzrs"]?.ToString() ?? "0";
                string sksj = course["sksj"]?.ToString() ?? "未知时间";
                string jxdd = course["jxdd"]?.ToString() ?? "未知教室";
                string kcxzmc = course["kcxzmc"]?.ToString() ?? "未知课程性质";

                // 计算剩余人数
                int reNumInt;
                object reNum = null;
                if (Int32.TryParse(jxbrl, out int jxbrlInt) && Int32.TryParse(yxzrs, out int yxzrsInt))
                {
                    reNumInt = jxbrlInt - yxzrsInt;
                    reNum = reNumInt;
                }
                else
                {
                    reNum = "无法计算";
                }

                // 存储提取的信息
                Dictionary<string, object> course_info = new Dictionary<string, object>()
                {
                    { "courseName", courseNameLocal },
                    { "teacher", jsxx },
                    { "allNum", jxbrl },
                    { "seNum", yxzrs },
                    { "reNum", reNum },
                    { "time", sksj },
                    { "classroom", jxdd },
                    { "classProp", kcxzmc },
                    { "kch_id", kch_id },
                    { "do_jxb_id", do_jxb_id }
                };

                idMapping[jxb_id] = course_info;

                // 输出课程信息
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("-------------------------------------");
                Console.WriteLine($"- 课程ID: {jxb_id}, 课程名称: {courseNameLocal}");
                
                Console.WriteLine($"  教师: {jsxx}");
                Console.WriteLine($"  总人数: {jxbrl}");
                Console.WriteLine($"  已选人数: {yxzrs}");

                // 判断剩余人数
                var leNum = reNum;
                if (courseType == "1")
                {
                    if (leNum is int l1 && l1 == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"  剩余人数: {l1}");
                        Console.WriteLine($"  请注意: 该课程人数已满, 选择后, 提交选课将会失败!");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.WriteLine($"  剩余人数: {leNum}");
                        Console.ResetColor();
                    }
                }
                else
                {
                    if (leNum is int l2)
                    {
                        if (l2 < 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"  超出课程人数: {Math.Abs(l2)}");
                            Console.ResetColor();
                        }
                        else if (l2 == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"  剩余人数: {l2}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkBlue;
                            Console.WriteLine($"  剩余人数: {l2}");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"  剩余人数: {leNum}");
                        Console.ResetColor();
                    }
                }
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine($"  时间: {sksj}");
                Console.WriteLine($"  教室: {jxdd}");
                Console.WriteLine($"  课程性质: {kcxzmc}");
                idx++;
            }

            Console.WriteLine("-------------------------------------\n");
            Console.ResetColor();
            Console.WriteLine("[INFO] 可选课程加载完毕");

            // 让用户选择课程并添加到选课列表
            PostCourse(idMapping, selectedCourses, category);
        }

        // postCourse
        static bool PostCourse(Dictionary<string, Dictionary<string, object>> idMapping,
                               Dictionary<string, List<Dictionary<string, object>>> selectedCourses,
                               string category)
        {
            if (Debug) {
                // 添加调试信息：列出所有可用的 jxb_id
                Console.WriteLine("\n[DEBUG] 可用的课程ID列表:");
                foreach (var key in idMapping.Keys)
                {
                Console.WriteLine($"  - {key}");
                }
                Console.WriteLine("-------------------------------------");
            }
            
            while (true)
            {
                Console.Write("[STEP] 请输入所需选择的课程ID (或输入 'back' 返回上一级菜单): \n");
                string courseID = Console.ReadLine().Trim();

                if (courseID.ToLower().Equals("back", StringComparison.OrdinalIgnoreCase))
                {
                    return false;  // 返回主菜单
                }

                // 检查是否存在对应的课程ID（不区分大小写）
                if (idMapping.ContainsKey(courseID))
                {
                    Console.WriteLine($"[STEP] 开始选择课程ID: {courseID} ...");
                    var courseInfo = idMapping[courseID];
                    string courseNameLocal = courseInfo.ContainsKey("courseName") ? (string)courseInfo["courseName"] : "未知课程名称";

                    // 检查是否已经选择了该课程
                    bool duplicate = selectedCourses.ContainsKey(category) &&
                                      selectedCourses[category].Any(c => c["jxb_id"].ToString().Equals(courseID, StringComparison.OrdinalIgnoreCase));
                    if (duplicate)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ERROR] 该课程已在选课列表中。\n");
                        Console.ResetColor();
                        return false;
                    }

                    // 添加课程到选课列表
                    if (!selectedCourses.ContainsKey(category))
                    {
                        selectedCourses[category] = new List<Dictionary<string, object>>();
                    }

                    var newCourse = new Dictionary<string, object>()
                    {
                        { "jxb_ids", courseInfo.ContainsKey("do_jxb_id") ? courseInfo["do_jxb_id"] : "" },
                        { "jxb_id", courseID },
                        { "kch_id", courseInfo.ContainsKey("kch_id") ? courseInfo["kch_id"] : "" },
                        { "xkkz_id", xkkz_id },
                        { "course_name", courseNameLocal },
                        { "course_teacher", courseInfo.ContainsKey("teacher") ? courseInfo["teacher"] : "未知教师" },
                        { "course_allnum", courseInfo.ContainsKey("allNum") ? courseInfo["allNum"] : "0" },
                        { "course_senum", courseInfo.ContainsKey("seNum") ? courseInfo["seNum"] : "0" },
                        { "course_time", courseInfo.ContainsKey("time") ? courseInfo["time"] : "未知时间" },
                        { "course_room", courseInfo.ContainsKey("classroom") ? courseInfo["classroom"] : "未知教室" }
                    };

                    selectedCourses[category].Add(newCourse);
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.WriteLine($"[SUCCESS] 课程 {courseID} 已添加到选课列表。");
                    Console.ResetColor();
                    return true;
                }
                else
                {
                    Console.WriteLine("\u001b[31m[ERROR] 未找到对应的课程ID。请确认输入是否正确或输入 'back' 返回上一级菜单。\u001b[0m\n");
                }
            }
        }

        // submitSelectedCourses
        static void SubmitSelectedCourses(Dictionary<string, List<Dictionary<string, object>>> selectedCourses)
        {
            Console.WriteLine("\n[INFO] 正在提交选课请求...");
            foreach (var kv in selectedCourses)
            {
                string category = kv.Key;
                var courses = kv.Value;
                foreach (var course in courses)
                {
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.WriteLine($"[INFO] 正在提交类别 '{category}' 的课程: \n- ID={course["jxb_id"]}\n- 课程名称: {course["course_name"]}\n- 课程老师: {course["course_teacher"]}...");
                    Console.ResetColor();
                    postCourseData["jxb_ids"] = course["jxb_ids"].ToString();
                    postCourseData["xkkz_id"] = course["xkkz_id"].ToString();
                    postCourseData["kch_id"] = course["kch_id"].ToString();

                    if (Debug) {
                        // 调试输出
                        Console.WriteLine("jxb-ids: " + postCourseData["jxb_ids"]);
                        Console.WriteLine("kch-id: " + postCourseData["kch_id"]);
                        Console.WriteLine("jxb-id: " + course["jxb_id"].ToString());
                    }

                    var p = new PostClass(postCourseURL, postCourseData, courseHeaders);
                    string respString = p.Post();
                    try
                    {
                        var respJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(respString);
                        if (Debug) {
                            Console.WriteLine("[DEBUG] 响应JSON: " + JsonConvert.SerializeObject(respJson));
                        }
                        if (respJson.ContainsKey("flag") && (string)respJson["flag"] == "1")
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"[SUCCESS] {course["course_name"]} 选课成功！");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"[ERROR] {course["course_name"]} 选课失败, flag = " + (respJson.ContainsKey("flag") ? respJson["flag"].ToString() : "null") + "\n[ERROR] 原因:" + (respJson.ContainsKey("msg") ? respJson["msg"].ToString() : "null"));
                            Console.ResetColor();
                            
                        }
                    }
                    catch (JsonReaderException)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ERROR] 响应文本:" + respString);
                        Console.ResetColor();
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[SUCCESS] 所有选课请求已提交完成！");
            Console.ResetColor();
            // 清空选课列表
            selectedCourses.Clear();
        }

        // deleteSelectedCourse
        static void DeleteSelectedCourse(Dictionary<string, List<Dictionary<string, object>>> selectedCourses)
        {
            if (selectedCourses.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[INFO] 当前选课列表为空，无需删除。");
                Console.ResetColor();
                return;
            }
            Console.WriteLine("\n[INFO] 已选择的课程列表:");
            Console.WriteLine("-------------------------------------");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            var categories = selectedCourses.Keys.ToList();
            for (int i = 0; i < categories.Count; i++)
            {
                string cat = categories[i];
                Console.WriteLine($"{i + 1}. 类别: {cat}");
                var cList = selectedCourses[cat];
                for (int j = 0; j < cList.Count; j++)
                {
                    var course = cList[j];
                    Console.WriteLine($"   {j + 1}. 课程ID: {course["jxb_id"]}, 课程名称: {course.GetValueOrDefault("course_name", "未知")}");
                    Console.ResetColor();
                }
            }
            

            while (true)
            {
                Console.Write("请输入要删除的课程编号 (格式 类别编号.课程编号，例如1.2) (或输入 'back' 返回主菜单): ");
                string input = Console.ReadLine().Trim().ToLower();
                if (input == "back") return;

                var match = Regex.Match(input, @"^(\d+)\.(\d+)$");
                if (match.Success)
                {
                    int cat_idx = int.Parse(match.Groups[1].Value) - 1;
                    int course_idx = int.Parse(match.Groups[2].Value) - 1;
                    if (cat_idx >= 0 && cat_idx < categories.Count)
                    {
                        string cat = categories[cat_idx];
                        var cList = selectedCourses[cat];
                        if (course_idx >= 0 && course_idx < cList.Count)
                        {
                            var del_course = cList[course_idx];
                            cList.RemoveAt(course_idx);
                            Console.ForegroundColor = ConsoleColor.DarkBlue;
                            Console.WriteLine($"[SUCCESS] 已删除类别 '{cat}' 的课程 '{del_course["jxb_id"]}'。\n");
                            Console.ResetColor();
                            if (cList.Count == 0)
                            {
                                selectedCourses.Remove(cat);
                            }
                            break;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[ERROR] 课程编号超出范围，请重新输入。\n");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ERROR] 类别编号超出范围，请重新输入。\n");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] 输入格式不正确，请输入如 '1.2' 或 'back'。\n");
                    Console.ResetColor();
                }
            }
        }

        // snatch_course_mode
        static void SnatchCourseMode(Dictionary<string, List<Dictionary<string, object>>> selectedCourses)
        {
            if (selectedCourses.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] 选课列表为空，请先添加课程。\n");
                Console.ResetColor();
                return;
            }
            while (true)
            {
                Console.Write("请输入抢课时间 (格式 HH:MM，例如 12:00) (或输入 'back' 返回主菜单): ");
                string t = Console.ReadLine().Trim().ToLower();
                if (t == "back") return;

                try
                {
                    // 解析时间
                    var now = DateTime.Now;
                    var parts = t.Split(':');
                    if (parts.Length != 2)
                        throw new FormatException();
                    int hh = int.Parse(parts[0]);
                    int mm = int.Parse(parts[1]);
                    var target_time = new DateTime(now.Year, now.Month, now.Day, hh, mm, 0);
                    if (target_time <= now)
                        target_time = target_time.AddDays(1);
                    Console.WriteLine($"[INFO] 抢课将在北京时间 {target_time:yyyy-MM-dd HH:mm:ss} 开始。");
                    // 启动监听线程
                    var cancelEvent = new ManualResetEvent(false);
                    Task.Run(() => {
                        while (true)
                        {
                            string userInput = Console.ReadLine().Trim().ToLower();
                            if (userInput == "back")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("[INFO] 已取消抢课模式，返回主菜单。");
                                Console.ResetColor();
                                cancelEvent.Set();
                                break;
                            }
                        }
                    });

                    while (true)
                    {
                        now = DateTime.Now;
                        double remaining = (target_time - now).TotalSeconds;
                        if (remaining > 5)
                        {
                            Console.WriteLine($"[WAIT] 当前时间: {now:yyyy-MM-dd HH:mm:ss}, 距离抢课还有 { (int)remaining } 秒。");
                            if (cancelEvent.WaitOne(2000))
                            {
                                return;
                            }
                        }
                        else if (remaining > 0 && remaining <= 5)
                        {
                            Console.WriteLine($"[INFO] 抢课开始！当前时间: {now:yyyy-MM-dd HH:mm:ss}");
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"[INFO] 抢课开始！当前时间: {now:yyyy-MM-dd HH:mm:ss}");
                            break;
                        }
                        if (cancelEvent.WaitOne(0))
                            return;
                    }
                    if (cancelEvent.WaitOne(0))
                        return;

                    // 多线程抢课
                    // 记录选课状态
                    Dictionary<string, bool> courseStatus = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                    foreach (var kv in selectedCourses)
                    {
                        foreach (var course in kv.Value)
                        {
                            courseStatus[(string)course["jxb_id"]] = false;
                        }
                    }

                    object lockObj = new object();
                    List<Thread> threads = new List<Thread>();

                    foreach (var kv in selectedCourses)
                    {
                        string cat = kv.Key;
                        foreach (var course in kv.Value)
                        {
                            var th = new Thread(() => {
                                SubmitCourseThread(cat, course, courseStatus, cancelEvent, lockObj);
                            });
                            th.Start();
                            threads.Add(th);
                        }
                    }

                    // 等待线程
                    foreach (var th in threads)
                    {
                        th.Join();
                    }

                    // 检查结果
                    bool allOk = courseStatus.Values.All(x => x == true);
                    if (allOk)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("[SUCCESS] 所有课程选课成功，抢课模式结束！");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ERROR] 部分课程选课未成功，抢课模式结束！");
                        Console.ResetColor();
                    }
                    return;
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] 时间格式不正确，请输入正确的格式 HH:MM。\n");
                    Console.ResetColor();
                }
            }
        }

        static void SubmitCourseThread(string category,
                                       Dictionary<string, object> course,
                                       Dictionary<string, bool> courseStatus,
                                       ManualResetEvent cancelEvent,
                                       object lockObj)
        {
            while (!courseStatus[(string)course["jxb_id"]])
            {
                if (cancelEvent.WaitOne(0))
                    break;
                // 提交请求
                postCourseData["jxb_ids"] = (string)course["jxb_ids"];
                postCourseData["xkkz_id"] = (string)course["xkkz_id"];
                postCourseData["kch_id"] = (string)course["kch_id"];
                if (Debug) {
                    Console.WriteLine($"[DEBUG] POST Body (Post): \n- jxb_ids: {postCourseData["jxb_ids"]}\n- xkkz_id: {postCourseData["xkkz_id"]}\n- kch_id: {postCourseData["kch_id"]}\n- jxb_id: {course["jxb_id"]}");
                    Thread.Sleep(3000);
                }
                
                // 同步发送
                var request = new HttpRequestMessage(HttpMethod.Post, postCourseURL);
                request.Content = new StringContent(BuildFormBody(postCourseData));
                var dictHeaders = new Dictionary<string, string>(courseHeaders);
                dictHeaders["Content-Type"] = "application/x-www-form-urlencoded";
                foreach (var kv in dictHeaders)
                {
                    if (kv.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    else
                        request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }
                try
                {
                    var response = httpClient.SendAsync(request).Result;
                    string respStr = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            var j = JsonConvert.DeserializeObject<Dictionary<string, object>>(respStr);
                            if (j != null && j.ContainsKey("flag") && (string)j["flag"] == "1")
                            {
                                lock (lockObj)
                                {
                                    courseStatus[(string)course["jxb_id"]] = true;
                                }
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"[SUCCESS] 课程 '{course["jxb_id"]}' 选课成功！");
                                Console.ResetColor();
                            }
                            else
                            {
                                var f = j != null && j.ContainsKey("flag") ? j["flag"] : null;
                                var msg = j!= null && j.ContainsKey("msg") ? j["msg"] : null;
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"[WARNING] 课程 '{course["course_name"]}' 选课失败, flag = {f}. 原因: {msg} 继续尝试...");
                                Console.ResetColor();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] 提交选课请求时返回非JSON或解析错误: {ex}, 继续尝试...");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] HTTP状态码 {response.StatusCode}, 响应文本: {respStr}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] 提交选课请求时出错: {ex}. 继续尝试...");
                }
                Thread.Sleep(1000);
            }
        }

        // ------------------- PostClass 对应 Python postClass ---------------

        class PostClass
        {
            private string URL;
            private Dictionary<string, string> Data;
            private Dictionary<string, string> Headers;

            public PostClass(string url, Dictionary<string, string> data, Dictionary<string, string> headers)
            {
                this.URL = url;
                this.Data = data;
                this.Headers = headers;
            }

            public JArray GetList()
            {
                try
                {
                    // 同步Post
                    var dictHeaders = new Dictionary<string, string>(Headers);
                    dictHeaders["Content-Type"] = "application/x-www-form-urlencoded";
                    string body = BuildFormBody(Data);

                    if (Debug) {
                        // 调试输出：显示发送的POST数据
                        Console.WriteLine($"[DEBUG] POST Body: {body}");
                    }
                    
                    var request = new HttpRequestMessage(HttpMethod.Post, URL);
                    request.Content = new StringContent(body);
                    foreach (var kv in dictHeaders)
                    {
                        if (kv.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                        else
                            request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                    }

                    var response = httpClient.SendAsync(request).Result;
                    Console.WriteLine($"[INFO] 当前操作状态码: {(int)response.StatusCode}");
                    string respString = response.Content.ReadAsStringAsync().Result;

                    if (Debug) {
                        Console.WriteLine($"[DEBUG] 响应内容: {respString}");
                    }
                    
                    try
                    {
                        var respJArray = JArray.Parse(respString);
                        Console.WriteLine("[INFO] 获取JSON成功");
                        return respJArray;
                    }
                    catch (JsonReaderException e)
                    {
                        Console.WriteLine("[ERROR] 响应文本:" + respString);
                        Console.WriteLine($"\u001b[31m[ERROR] JSON 解析失败: {e}\u001b[0m");
                        Environment.Exit(1);
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\u001b[31m[ERROR] 请求异常: {e}\u001b[0m");
                    Environment.Exit(1);
                    return null;
                }
            }

            public string GetHTML()
            {
                try
                {
                    var dictHeaders = new Dictionary<string, string>(Headers);
                    dictHeaders["Content-Type"] = "application/x-www-form-urlencoded";
                    string body = BuildFormBody(Data);

                    if (Debug) {
                        // 调试输出：显示发送的POST数据
                        Console.WriteLine($"[DEBUG] POST Body (GetHTML): {body}");
                    }
                    
                    var request = new HttpRequestMessage(HttpMethod.Post, URL);
                    request.Content = new StringContent(body);
                    foreach (var kv in dictHeaders)
                    {
                        if (kv.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                        else
                            request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                    }

                    var response = httpClient.SendAsync(request).Result;
                    Console.WriteLine($"[INFO] 当前操作状态码: {(int)response.StatusCode}");
                    string respString = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("[INFO] 获取HTML成功");
                    return respString;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\u001b[31m[ERROR] 请求或解析HTML过程中出错: {e}\u001b[0m");
                    Environment.Exit(1);
                    return null;
                }
            }

            public string Post()
            {
                // 发起请求
                Console.WriteLine("[INFO] 开始提交选课请求...");
                try
                {
                    var dictHeaders = new Dictionary<string, string>(Headers);
                    dictHeaders["Content-Type"] = "application/x-www-form-urlencoded";
                    string body = BuildFormBody(Data);

                    if (Debug) {
                        // 调试输出：显示发送的POST数据
                        Console.WriteLine($"[DEBUG] POST Body (Post): {body}");
                    }

                    var request = new HttpRequestMessage(HttpMethod.Post, URL);
                    request.Content = new StringContent(body);
                    foreach (var kv in dictHeaders)
                    {
                        if (kv.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                        else
                            request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                    }

                    var response = httpClient.SendAsync(request).Result;
                    Console.WriteLine("[INFO] 状态码: " + (int)response.StatusCode);
                    string respString = response.Content.ReadAsStringAsync().Result;
                    return respString;
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] 请求异常: " + e);
                    Console.ResetColor();
                    return "";
                }
            }
        }

        // ---------------- 工具：将 FormData 转成 x-www-form-urlencoded ----------------
        static string BuildFormBody(Dictionary<string, string> data)
        {
            List<string> list = new List<string>();
            foreach (var kv in data)
            {
                // 进行URL编码
                string key = Uri.EscapeDataString(kv.Key);
                string val = Uri.EscapeDataString(kv.Value);
                list.Add($"{key}={val}");
            }
            return string.Join("&", list);
        }

        // ------------------- 扩展方法类 -------------------
    }

    // 将 JsonExtensions 类移动到 Program 类之外，并声明为静态类
    public static class JsonExtensions
    {
        /// <summary>
        /// 获取JToken中指定键的值，如果不存在则返回默认值。
        /// </summary>
        /// <typeparam name="T">返回值的类型</typeparam>
        /// <param name="token">JToken对象</param>
        /// <param name="key">键名称</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>键对应的值或默认值</returns>
        public static T GetValueOrDefault<T>(this JToken token, string key, T defaultValue)
        {
            if (token[key] != null)
            {
                try
                {
                    return token[key].ToObject<T>();
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }
    }
}