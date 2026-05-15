namespace HabitFlow

open System
open Elmish

/// Elmish状态管理模块
/// 遵循Model-View-Update架构
/// Update函数为纯函数：无副作用，输入Msg和Model，返回新Model和可选Command
module State =

    // ===== 初始状态 =====

    /// 空Model（首次使用时的默认状态）
    let emptyModel = {
        Habits = []
        Filter = Active
        Sort = ByCreated
        EditingHabit = None
        IsAddModalOpen = false
        SelectedDate = DateTime.Now
        Today = DateTime.Now
    }

    // ===== 初始化 =====

    /// 初始化函数：尝试从localStorage加载持久化数据
    /// 返回 (Model, Cmd<Msg>) 元组，Elmish会调度命令
    let init () : Model * Cmd<Msg> =
        // 同步尝试加载；若失败则返回空Model
        match Storage.loadModel() with
        | Some loaded -> loaded, Cmd.none
        | None -> emptyModel, Cmd.none

    // ===== Update（核心纯函数）=====

    /// 纯函数：接收消息和当前模型，返回新模型和命令
    /// 所有状态变更都通过Record Copy-and-Update语法实现
    /// 所有副作用通过Elmish Command系统委托
    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        // ---- CRUD操作 ----

        /// 添加新习惯：将新习惯添加到列表头部，触发保存
        | AddHabit habit ->
            let newModel = { model with Habits = habit :: model.Habits }
            newModel, Cmd.ofMsg SaveToStorage

        /// 更新习惯：找到匹配ID的习惯并替换，关闭编辑模态框
        | UpdateHabit updatedHabit ->
            let newHabits =
                model.Habits
                |> List.map (fun h ->
                    if h.Id = updatedHabit.Id then updatedHabit else h)
            let newModel = {
                model with
                    Habits = newHabits
                    EditingHabit = None
            }
            newModel, Cmd.ofMsg SaveToStorage

        /// 删除习惯：过滤掉指定ID的习惯
        | DeleteHabit habitId ->
            let newHabits =
                model.Habits
                |> List.filter (fun h -> h.Id <> habitId)
            let newModel = { model with Habits = newHabits }
            newModel, Cmd.ofMsg SaveToStorage

        /// 归档习惯：切换指定习惯的归档状态
        | ArchiveHabit habitId ->
            let newHabits =
                model.Habits
                |> List.map (fun h ->
                    if h.Id = habitId then Logic.toggleArchive h else h)
            let newModel = { model with Habits = newHabits }
            newModel, Cmd.ofMsg SaveToStorage

        /// 恢复习惯：切换指定习惯的归档状态（与ArchiveHabit共用toggleArchive）
        | UnarchiveHabit habitId ->
            let newHabits =
                model.Habits
                |> List.map (fun h ->
                    if h.Id = habitId then Logic.toggleArchive h else h)
            let newModel = { model with Habits = newHabits }
            newModel, Cmd.ofMsg SaveToStorage

        // ---- 打卡操作 ----

        /// 打卡：为指定 habit 在指定日期添加打卡记录
        | CheckIn (habitId, date) ->
            let newHabits =
                model.Habits
                |> List.map (fun h ->
                    if h.Id = habitId then Logic.checkIn h date else h)
            let newModel = { model with Habits = newHabits }
            newModel, Cmd.ofMsg SaveToStorage

        /// 取消打卡：移除指定 habit 在指定日期的打卡记录
        | UncheckIn (habitId, date) ->
            let newHabits =
                model.Habits
                |> List.map (fun h ->
                    if h.Id = habitId then Logic.uncheckIn h date else h)
            let newModel = { model with Habits = newHabits }
            newModel, Cmd.ofMsg SaveToStorage

        // ---- UI操作（纯状态变更，无副作用） ----

        /// 打开添加习惯模态框
        | OpenAddModal ->
            { model with IsAddModalOpen = true }, Cmd.none

        /// 关闭添加习惯模态框
        | CloseAddModal ->
            { model with IsAddModalOpen = false }, Cmd.none

        /// 打开编辑习惯模态框：通过ID查找习惯放入EditingHabit
        | OpenEditModal habitId ->
            let habit =
                model.Habits
                |> List.tryFind (fun h -> h.Id = habitId)
            { model with EditingHabit = habit }, Cmd.none

        /// 关闭编辑习惯模态框：清除EditingHabit
        | CloseEditModal ->
            { model with EditingHabit = None }, Cmd.none

        /// 设置筛选条件
        | SetFilter filter ->
            { model with Filter = filter }, Cmd.none

        /// 设置排序方式
        | SetSort sort ->
            { model with Sort = sort }, Cmd.none

        /// 选择日期
        | SelectDate date ->
            { model with SelectedDate = date }, Cmd.none

        // ---- 存储操作 ----

        /// 从存储加载（init中已同步加载，此处为no-op）
        | LoadFromStorage ->
            model, Cmd.none

        /// 保存到存储：通过Cmd.ofSub委托副作用，保持Update纯函数
        | SaveToStorage ->
            model, Cmd.ofSub (fun _dispatch -> Storage.saveModel model)

        /// 存储加载完成（init中已处理，此处为no-op）
        | StorageLoaded _ ->
            model, Cmd.none
