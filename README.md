# 广州大学抢课脚本

![License](https://img.shields.io/badge/License-GPL3.0-green)

广州大学选课的抢课脚本，使用C#实现，可能适用于正方教务系统的其他学校

## 功能特性

✅ 多账号管理（使用playwright获取cookie）  
✅ 预选课程，到系统开放时间直接抢  
✅ 高并发抢课

## 未实现的功能

1. 可视化Webui
2. 换用post请求过CAS登录
3. 等下次抢课前再优化吧

## 最终用户许可协议 (EULA)  
### 重要事项  
> 请在使用本软件前仔细阅读本协议。安装或使用即视为同意条款，否则请勿使用。  

### 第1条 软件授权范围  
1. **非商业用途**  
   - 允许安装、使用、修改及创作衍生作品。  
2. **商业用途**  
   - 禁止未经授权的商业运营（如预装、捆绑）。  
3. **开源协议范围**  
   - 允许在 [AGPL-3.0](https://www.gnu.org/licenses/agpl-3.0.html) 协议下二次创作和分发。  

### 第2条 用户义务  
1. 禁止利用本软件从事违法或侵权行为。  
2. 未经许可不得用于盈利。  

### 第3条 免责声明  
1. 作者不提供技术支持，不保证软件无中断或错误。  
2. 因网络、设备等不可抗力造成的损失，作者不承担责任。  
3. 不保证通过本软件获取的信息的合法性或准确性。  

---

## 更新日志

### v0.2 版本  
- **0.2.10**  
  - [修复] 提交选课结果中课程名称未知的问题，增加失败原因说明。  
- **0.2.9**  
  - [新增] 支持预加载课程列表和已选课程列表。  
- **0.2.8**  
  - [优化] 筛选仅显示有余量的课程。  
- **0.2.7**  
  - [新增] 缓存课程数据，避免重复请求。  
- **0.2.6**  
  - [优化] 初始化时获取ID，提升稳定性。  
- **0.2.5**  
  - [新增] 自动获取通用ID，适配所有用户和年份。  
- **0.2.4**  
  - [修复] 抢课请求头格式错误导致提交失败的问题。  
- **0.2.3**  
  - [预发布] 修复拼写错误。  
- **0.2.1**  
  - [重构] 使用 C# 重写脚本，采用 Playwright 驱动浏览器。  
- **0.2.0**  
  - [重构] 全面迁移至 C# 语言。  

### v0.1 版本（Python 原版）  
- **0.1.9**  
  - [新增] 抢课模式支持 2 秒刷新频率，可随时中断返回菜单。  
- **0.1.8**  
  - [新增] 抢课模式倒计时和多线程提交。  
- **0.1.7**  
  - [新增] 支持删除已选课程和全局返回逻辑。  
- **0.1.6**  
  - [新增] 多选课程并批量提交功能。  
- **0.1.5**  
  - [优化] 新增菜单层级和返回逻辑。  
- **0.1.4**  
  - [新增] 广州大学 CAS 自动登录，无需手动抓取 Cookie。  
- **0.1.3**  
  - [修复] 课表获取逻辑和提交逻辑。  
- **0.1.2**  
  - [新增] 支持查看主修课程表。  
- **0.1.1**  
  - [新增] 通识选修课程列表和选课功能。  
- **0.1.0**  
  - [初始版本] Python 构建的基础功能。  

---

⚠️ **提示**: 使用前请确保已阅读并同意 [EULA](#最终用户许可协议-eula) 条款。  


## 开源协议
[GPL-3.0 License](LICENSE)
