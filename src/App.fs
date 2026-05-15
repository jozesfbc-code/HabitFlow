namespace HabitFlow

open Elmish
open Elmish.React

/// 应用程序入口模块
/// 将Elmish的Model、Update、View三部分组装为可运行程序
/// 使用React作为渲染目标，挂载到id="root"的DOM节点
module App =

    /// 启动Elmish程序
    /// mkProgram: 将init、update、render三个核心函数组装为Program实例
    /// withReactSynchronous: 使用React同步渲染模式，挂载到id="root"的元素
    /// run: 启动事件循环，开始处理消息
    Program.mkProgram State.init State.update View.render
    |> Program.withReactSynchronous "root"
    |> Program.run
