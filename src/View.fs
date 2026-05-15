namespace HabitFlow

open System
open Feliz
open Elmish
open Browser

/// Feliz视图模块
/// 所有组件为纯函数，接收model和dispatch，返回ReactElement
/// 使用Feliz DSL：Html.div [ prop.children [...] ] 风格
module View =

    // ===== 辅助函数（纯函数） =====

    /// 习惯类别到CSS类名的映射
    let private categoryClass = function
        | Health       -> "category-health"
        | Productivity -> "category-productivity"
        | Learning     -> "category-learning"
        | Creativity   -> "category-creativity"
        | Social       -> "category-social"
        | Wellness     -> "category-wellness"

    /// 习惯类别到显示名称的映射
    let private categoryName = function
        | Health       -> "Health"
        | Productivity -> "Productivity"
        | Learning     -> "Learning"
        | Creativity   -> "Creativity"
        | Social       -> "Social"
        | Wellness     -> "Wellness"

    /// 图标到Emoji的映射（纯展示，无状态）
    let private iconEmoji = function
        | Droplet  -> "💧"
        | Book     -> "📚"
        | Dumbbell -> "💪"
        | Code     -> "💻"
        | Sun      -> "☀️"
        | Moon     -> "🌙"
        | Heart    -> "❤️"
        | Star     -> "⭐"
        | Zap      -> "⚡"
        | Music    -> "🎵"

    /// 检查某习惯在指定日期是否已打卡（高阶函数：List.exists）
    let private isCheckedToday (habit: Habit) (date: DateTime) : bool =
        habit.CheckIns |> List.exists (fun d -> Logic.isSameDay d date)

    // ===== 头部组件 =====

    /// 应用头部：标题 + 副标题
    let private headerView () =
        Html.header [
            prop.className "app-header"
            prop.children [
                Html.h1 "HabitFlow"
                Html.p "Build better habits, one day at a time"
            ]
        ]

    // ===== 统计概览组件 =====

    /// 统计网格：展示习惯总数、今日完成、平均连续天数、总打卡次数
    /// 使用List.filter、List.sumBy、List.length等高阶函数计算统计值
    let private statsOverviewView (habits: Habit list) (today: DateTime) =
        let activeHabits = habits |> List.filter (fun h -> not h.Archived)
        let totalHabits  = activeHabits.Length
        let totalCheckins =
            activeHabits |> List.sumBy (fun h -> h.CheckIns.Length)
        let avgStreak =
            if activeHabits.IsEmpty then 0
            else
                let sumStreaks =
                    activeHabits
                    |> List.sumBy (fun h -> Logic.calculateStreak h today)
                sumStreaks / activeHabits.Length
        let todayHabits = Logic.getTodaysHabits today activeHabits
        let completedToday =
            todayHabits
            |> List.filter (fun h -> isCheckedToday h today)
            |> List.length

        /// 单个统计卡片组件
        let statCard value label =
            Html.div [
                prop.className "stat-card"
                prop.children [
                    Html.div [ prop.className "stat-value"; prop.text value ]
                    Html.div [ prop.className "stat-label";  prop.text label  ]
                ]
            ]

        Html.div [
            prop.className "stats-grid"
            prop.children [
                statCard (string totalHabits) "Habits"
                statCard (sprintf "%d/%d" completedToday todayHabits.Length) "Today"
                statCard (string avgStreak) "Avg Streak"
                statCard (string totalCheckins) "Check-ins"
            ]
        ]

    // ===== 工具栏组件 =====

    /// 筛选下拉框 + 排序下拉框 + 添加按钮
    let private toolbarView (currentFilter: FilterOption)
                            (currentSort: SortOption)
                            (dispatch: Msg -> unit) =

        /// 筛选选项的显示名称（用于匹配select的值）
        let filterDisplayName = function
            | All                     -> "All Habits"
            | Active                  -> "Active"
            | Archived                -> "Archived"
            | ByCategoryFilter Health       -> "Health"
            | ByCategoryFilter Productivity -> "Productivity"
            | ByCategoryFilter Learning     -> "Learning"
            | ByCategoryFilter Creativity   -> "Creativity"
            | ByCategoryFilter Social       -> "Social"
            | ByCategoryFilter Wellness     -> "Wellness"

        /// 排序选项的显示名称
        let sortDisplayName = function
            | ByCreated -> "Newest"
            | ByName    -> "Name"
            | ByCategory -> "Category"
            | ByStreak  -> "Streak"

        /// 从select的字符串值反解析为FilterOption（模式匹配穷尽性）
        let parseFilter (v: string) : FilterOption =
            match v with
            | "Active"       -> Active
            | "Archived"     -> Archived
            | "Health"       -> ByCategoryFilter Health
            | "Productivity" -> ByCategoryFilter Productivity
            | "Learning"     -> ByCategoryFilter Learning
            | "Creativity"   -> ByCategoryFilter Creativity
            | "Social"       -> ByCategoryFilter Social
            | "Wellness"     -> ByCategoryFilter Wellness
            | _              -> All   // "All Habits" 或其他

        /// 从select的字符串值反解析为SortOption
        let parseSort (v: string) : SortOption =
            match v with
            | "Name"     -> ByName
            | "Category" -> ByCategory
            | "Streak"   -> ByStreak
            | _          -> ByCreated   // "Newest" 或其他

        /// 生成select选项元素（高阶函数：List.map）
        let selectOption valueText =
            Html.option [ prop.value valueText; prop.text valueText ]

        Html.div [
            prop.className "toolbar"
            prop.children [
                // 筛选下拉框
                Html.select [
                    prop.value (filterDisplayName currentFilter)
                    prop.onChange (fun (e: Browser.Types.Event) ->
                        let v = unbox<string> e.currentTarget?value
                        dispatch (SetFilter (parseFilter v)))
                    prop.children [
                        selectOption "All Habits"
                        selectOption "Active"
                        selectOption "Archived"
                        selectOption "Health"
                        selectOption "Productivity"
                        selectOption "Learning"
                        selectOption "Creativity"
                        selectOption "Social"
                        selectOption "Wellness"
                    ]
                ]
                // 排序下拉框
                Html.select [
                    prop.value (sortDisplayName currentSort)
                    prop.onChange (fun (e: Browser.Types.Event) ->
                        let v = unbox<string> e.currentTarget?value
                        dispatch (SetSort (parseSort v)))
                    prop.children [
                        selectOption "Newest"
                        selectOption "Name"
                        selectOption "Category"
                        selectOption "Streak"
                    ]
                ]
                // 弹性占位
                Html.div [ prop.style [ style.flexGrow 1 ] ]
                // 添加习惯按钮
                Html.button [
                    prop.className "btn-primary"
                    prop.text "+ Add Habit"
                    prop.onClick (fun _ -> dispatch OpenAddModal)
                ]
            ]
        ]

    // ===== 打卡按钮组件 =====

    /// 习惯卡片的打卡按钮：显示为图标或勾选标记
    /// 点击切换打卡/取消打卡状态
    let private checkInButton (habit: Habit) (today: DateTime) (dispatch: Msg -> unit) =
        let checked = isCheckedToday habit today
        let btnClass =
            if checked then "checkin-btn checked" else "checkin-btn"
        Html.button [
            prop.className btnClass
            prop.text (if checked then "✓" else iconEmoji habit.Icon)
            prop.title (if checked then "Click to uncheck" else "Click to check in")
            prop.onClick (fun _ ->
                if checked
                then dispatch (UncheckIn (habit.Id, today))
                else dispatch (CheckIn (habit.Id, today)))
        ]

    // ===== 周进度组件 =====

    /// 展示本周7天的打卡进度（7个小圆点）
    /// 使用Logic.getWeeklyProgress获取数据
    let private weekProgressView (habit: Habit) (today: DateTime) =
        let progress = Logic.getWeeklyProgress habit today
        Html.div [
            prop.className "week-progress"
            prop.children (
                // 高阶函数：List.map将进度数据渲染为UI元素
                progress |> List.map (fun (day, done_) ->
                    let dotClass =
                        if done_ then "week-day-dot done" else "week-day-dot"
                    Html.div [
                        prop.className "week-day"
                        prop.children [
                            Html.div [
                                prop.className "week-day-label"
                                prop.text (Logic.getDayAbbrev day)
                            ]
                            Html.div [ prop.className dotClass ]
                        ]
                    ]))
        ]

    // ===== 连续天数徽章 =====

    /// 火焰徽章：当连续天数>0时显示🔥+天数
    let private streakBadgeView (streak: int) =
        if streak > 0 then
            Html.span [
                prop.className "streak-badge"
                prop.children [
                    Html.span [ prop.className "flame-icon"; prop.text "🔥" ]
                    Html.span (string streak)
                ]
            ]
        else
            Html.none

    // ===== 习惯卡片组件 =====

    /// 单个习惯卡片：标题、类别、操作按钮、打卡按钮、连续天数、周进度
    /// 组合多个子组件，CSS类名包含类别颜色和归档状态
    let private habitCardView (habit: Habit) (today: DateTime) (dispatch: Msg -> unit) =
        let stats = Logic.getStats habit today

        // 组合CSS类名
        let cardClasses =
            let baseClass = "habit-card " + categoryClass habit.Category
            if habit.Archived then baseClass + " archived" else baseClass

        Html.div [
            prop.className cardClasses
            prop.children [
                // 头部：标题 + 操作按钮
                Html.div [
                    prop.className "habit-header"
                    prop.children [
                        Html.div [
                            prop.children [
                                Html.div [
                                    prop.className "habit-title"
                                    prop.text habit.Name
                                ]
                                Html.div [
                                    prop.className "habit-category"
                                    prop.text (categoryName habit.Category)
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "habit-actions"
                            prop.children [
                                Html.button [
                                    prop.text "✏️"
                                    prop.title "Edit"
                                    prop.onClick (fun _ ->
                                        dispatch (OpenEditModal habit.Id))
                                ]
                                Html.button [
                                    prop.text (if habit.Archived then "📥" else "📤")
                                    prop.title (if habit.Archived then "Unarchive"
                                                else "Archive")
                                    prop.onClick (fun _ ->
                                        if habit.Archived
                                        then dispatch (UnarchiveHabit habit.Id)
                                        else dispatch (ArchiveHabit habit.Id))
                                ]
                                Html.button [
                                    prop.text "🗑️"
                                    prop.title "Delete"
                                    prop.onClick (fun _ ->
                                        if Dom.window.confirm(
                                            sprintf "Delete '%s'?" habit.Name)
                                        then dispatch (DeleteHabit habit.Id))
                                ]
                            ]
                        ]
                    ]
                ]
                // 打卡按钮
                checkInButton habit today dispatch
                // 底部信息行：连续徽章 + 总打卡数
                Html.div [
                    prop.style [
                        style.display.flex
                        style.justifyContent.spaceBetween
                        style.alignItems.center
                    ]
                    prop.children [
                        streakBadgeView stats.CurrentStreak
                        Html.span [
                            prop.style [
                                style.fontSize (length.px 12)
                                style.color "var(--text-muted)"
                            ]
                            prop.text (sprintf "%d total" stats.TotalCheckIns)
                        ]
                    ]
                ]
                // 周进度条
                weekProgressView habit today
            ]
        ]

    // ===== 习惯列表组件 =====

    /// 习惯网格容器：将过滤排序后的习惯列表渲染为卡片网格
    /// 高阶函数：List.map将每个Habit映射为habitCardView
    let private habitListView (habits: Habit list) (today: DateTime)
                              (dispatch: Msg -> unit) =
        Html.div [
            prop.className "habit-grid"
            prop.children (habits |> List.map (fun h ->
                habitCardView h today dispatch))
        ]

    // ===== 空状态组件 =====

    /// 当没有任何习惯时的引导界面
    let private emptyStateView (dispatch: Msg -> unit) =
        Html.div [
            prop.className "empty-state"
            prop.children [
                Html.div [ prop.className "empty-state-icon"; prop.text "🌱" ]
                Html.h3 "No habits yet"
                Html.p "Start building better habits by adding your first one!"
                Html.button [
                    prop.className "btn-primary"
                    prop.style [ style.marginTop (length.px 16) ]
                    prop.text "Add Your First Habit"
                    prop.onClick (fun _ -> dispatch OpenAddModal)
                ]
            ]
        ]

    // ===== 习惯表单模态框 =====

    /// 添加/编辑习惯的模态框表单
    /// 使用React.useState管理表单本地状态
    /// 新建时所有字段有默认值，编辑时预填充 habit 数据
    let private habitFormModal (editingHabit: Habit option)
                               (isOpen: bool)
                               (dispatch: Msg -> unit) =
        if not isOpen then
            Html.none
        else
            let isEdit = Option.isSome editingHabit

            // 编辑时取现有 habit，新建时用默认值
            let habit =
                editingHabit
                |> Option.defaultValue (
                    Logic.createHabit "" Health Daily Emerald Droplet)

            // React状态钩子管理表单字段（本地UI状态）
            let (name, setName)             = React.useState habit.Name
            let (category, setCategory)     = React.useState habit.Category
            let (frequency, setFrequency)   = React.useState habit.Frequency
            let (color, setColor)           = React.useState habit.Color
            let (icon, setIcon)             = React.useState habit.Icon

            /// 提交处理：构建Habit record并发送对应Msg
            let handleSubmit (e: Browser.Types.Event) =
                e.preventDefault()
                if not (String.IsNullOrWhiteSpace name) then
                    let updatedHabit =
                        if isEdit then
                            // Record Copy-and-Update：保留Id和CreatedAt等不变字段
                            { habit with
                                Name      = name.Trim()
                                Category  = category
                                Frequency = frequency
                                Color     = color
                                Icon      = icon
                            }
                        else
                            Logic.createHabit (name.Trim()) category frequency color icon
                    // 根据模式分发不同消息
                    if isEdit
                    then dispatch (UpdateHabit updatedHabit)
                    else dispatch (AddHabit updatedHabit)
                    if not isEdit then setName ""
                    dispatch CloseAddModal
                    dispatch CloseEditModal

            /// 关闭模态框
            let handleClose () =
                dispatch CloseAddModal
                dispatch CloseEditModal

            // 频率的select显示值
            let freqValue =
                match frequency with
                | Daily           -> "daily"
                | SpecificDays _  -> "specific"
                | TimesPerWeek _  -> "weekly"

            Html.div [
                prop.className "modal-overlay"
                // 点击遮罩层关闭
                prop.onClick (fun e ->
                    if e.target = e.currentTarget then handleClose())
                prop.children [
                    Html.form [
                        prop.className "modal-content"
                        prop.onSubmit handleSubmit
                        prop.children [
                            Html.h2 (if isEdit then "Edit Habit" else "New Habit")

                            // ---- 名称输入 ----
                            Html.div [
                                prop.className "form-group"
                                prop.children [
                                    Html.label "Habit Name"
                                    Html.input [
                                        prop.type' "text"
                                        prop.placeholder "e.g., Read 30 minutes"
                                        prop.value name
                                        prop.onChange setName
                                        prop.autoFocus true
                                    ]
                                ]
                            ]

                            // ---- 类别 + 颜色（同一行） ----
                            Html.div [
                                prop.className "form-row"
                                prop.children [
                                    Html.div [
                                        prop.className "form-group"
                                        prop.children [
                                            Html.label "Category"
                                            Html.select [
                                                prop.value (string category)
                                                prop.onChange (
                                                    fun (e: Browser.Types.Event) ->
                                                        match unbox<string> e.currentTarget?value with
                                                        | "Health"       -> setCategory Health
                                                        | "Productivity" -> setCategory Productivity
                                                        | "Learning"     -> setCategory Learning
                                                        | "Creativity"   -> setCategory Creativity
                                                        | "Social"       -> setCategory Social
                                                        | _              -> setCategory Wellness)
                                                prop.children [
                                                    Html.option [ prop.value "Health";       prop.text "Health"       ]
                                                    Html.option [ prop.value "Productivity"; prop.text "Productivity" ]
                                                    Html.option [ prop.value "Learning";     prop.text "Learning"     ]
                                                    Html.option [ prop.value "Creativity";   prop.text "Creativity"   ]
                                                    Html.option [ prop.value "Social";       prop.text "Social"       ]
                                                    Html.option [ prop.value "Wellness";     prop.text "Wellness"     ]
                                                ]
                                            ]
                                        ]
                                    ]
                                    Html.div [
                                        prop.className "form-group"
                                        prop.children [
                                            Html.label "Color"
                                            Html.select [
                                                prop.value (string color)
                                                prop.onChange (
                                                    fun (e: Browser.Types.Event) ->
                                                        match unbox<string> e.currentTarget?value with
                                                        | "Emerald" -> setColor Emerald
                                                        | "Blue"    -> setColor Blue
                                                        | "Amber"   -> setColor Amber
                                                        | "Rose"    -> setColor Rose
                                                        | "Violet"  -> setColor Violet
                                                        | _         -> setColor Teal)
                                                prop.children [
                                                    Html.option [ prop.value "Emerald"; prop.text "Emerald" ]
                                                    Html.option [ prop.value "Blue";    prop.text "Blue"    ]
                                                    Html.option [ prop.value "Amber";   prop.text "Amber"   ]
                                                    Html.option [ prop.value "Rose";    prop.text "Rose"    ]
                                                    Html.option [ prop.value "Violet";  prop.text "Violet"  ]
                                                    Html.option [ prop.value "Teal";    prop.text "Teal"    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]

                            // ---- 频率选择 ----
                            Html.div [
                                prop.className "form-group"
                                prop.children [
                                    Html.label "Frequency"
                                    Html.select [
                                        prop.value freqValue
                                        prop.onChange (
                                            fun (e: Browser.Types.Event) ->
                                                match unbox<string> e.currentTarget?value with
                                                | "daily"    -> setFrequency Daily
                                                | "specific" -> setFrequency (SpecificDays [Mon; Wed; Fri])
                                                | _          -> setFrequency (TimesPerWeek 3))
                                        prop.children [
                                            Html.option [ prop.value "daily";    prop.text "Every Day"      ]
                                            Html.option [ prop.value "specific"; prop.text "Specific Days"  ]
                                            Html.option [ prop.value "weekly";   prop.text "Times Per Week" ]
                                        ]
                                    ]
                                ]
                            ]

                            // ---- 图标选择 ----
                            Html.div [
                                prop.className "form-group"
                                prop.children [
                                    Html.label "Icon"
                                    Html.select [
                                        prop.value (string icon)
                                        prop.onChange (
                                            fun (e: Browser.Types.Event) ->
                                                match unbox<string> e.currentTarget?value with
                                                | "Droplet"  -> setIcon Droplet
                                                | "Book"     -> setIcon Book
                                                | "Dumbbell" -> setIcon Dumbbell
                                                | "Code"     -> setIcon Code
                                                | "Sun"      -> setIcon Sun
                                                | "Moon"     -> setIcon Moon
                                                | "Heart"    -> setIcon Heart
                                                | "Star"     -> setIcon Star
                                                | "Zap"      -> setIcon Zap
                                                | _          -> setIcon Music)
                                        prop.children [
                                            Html.option [ prop.value "Droplet";  prop.text "💧 Droplet"  ]
                                            Html.option [ prop.value "Book";     prop.text "📚 Book"     ]
                                            Html.option [ prop.value "Dumbbell"; prop.text "💪 Dumbbell" ]
                                            Html.option [ prop.value "Code";     prop.text "💻 Code"     ]
                                            Html.option [ prop.value "Sun";      prop.text "☀️ Sun"      ]
                                            Html.option [ prop.value "Moon";     prop.text "🌙 Moon"     ]
                                            Html.option [ prop.value "Heart";    prop.text "❤️ Heart"    ]
                                            Html.option [ prop.value "Star";     prop.text "⭐ Star"     ]
                                            Html.option [ prop.value "Zap";      prop.text "⚡ Zap"      ]
                                            Html.option [ prop.value "Music";    prop.text "🎵 Music"    ]
                                        ]
                                    ]
                                ]
                            ]

                            // ---- 操作按钮 ----
                            Html.div [
                                prop.className "modal-actions"
                                prop.children [
                                    Html.button [
                                        prop.className "btn-secondary"
                                        prop.type' "button"
                                        prop.text "Cancel"
                                        prop.onClick (fun _ -> handleClose())
                                    ]
                                    Html.button [
                                        prop.className "btn-primary"
                                        prop.type' "submit"
                                        prop.text (if isEdit then "Save Changes"
                                                   else "Create Habit")
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]

    // ===== 底部组件 =====

    /// 页脚信息
    let private footerView () =
        Html.footer [
            prop.className "app-footer"
            prop.children [
                Html.span "HabitFlow • Built with F#, Elmish & Functional Programming"
            ]
        ]

    // ===== 主渲染函数（组合所有组件） =====

    /// 主视图：组合头部、统计、工具栏、习惯列表/空状态、底部、模态框
    /// 函数组合：filterHabits >> sortHabits 管道处理
    let render (model: Model) (dispatch: Msg -> unit) =
        // 函数组合：先筛选再排序（管道操作）
        let filteredHabits =
            model.Habits
            |> Logic.filterHabits model.Filter
            |> Logic.sortHabits model.Sort

        // 模态框显示条件：添加模态框打开 或 正在编辑某个习惯
        let showModal = model.IsAddModalOpen || Option.isSome model.EditingHabit

        Html.div [
            prop.className "app-container"
            prop.children [
                // 1. 头部
                headerView ()

                // 2. 统计概览（基于所有习惯）
                statsOverviewView model.Habits model.Today

                // 3. 工具栏（筛选/排序/添加）
                toolbarView model.Filter model.Sort dispatch

                // 4. 习惯列表或空状态（条件渲染）
                if filteredHabits.IsEmpty && model.Habits.IsEmpty then
                    // 全局空状态：没有任何习惯
                    emptyStateView dispatch
                elif filteredHabits.IsEmpty then
                    // 筛选结果为空：有习惯但当前筛选无匹配
                    Html.div [
                        prop.className "empty-state"
                        prop.children [
                            Html.div [
                                prop.className "empty-state-icon"
                                prop.text "🔍"
                            ]
                            Html.h3 "No matching habits"
                            Html.p "Try adjusting your filter or add a new habit."
                        ]
                    ]
                else
                    // 正常展示习惯网格
                    habitListView filteredHabits model.Today dispatch

                // 5. 底部
                footerView ()

                // 6. 模态框（条件渲染）
                habitFormModal model.EditingHabit showModal dispatch
            ]
        ]
