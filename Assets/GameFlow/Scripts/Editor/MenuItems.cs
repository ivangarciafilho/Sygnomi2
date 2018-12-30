// [!] DO NOT REMOVE THIS FILE

using UnityEngine;
using UnityEditor;

namespace GameFlow {

public class MenuItems {

// Assets > Create > GameFlow

[MenuItem("Assets/Create/GameFlow/Action...", false, 3100)]
static void Assets_CreateAction() {
	AssetsMenuItems.CreateAction();
}

[MenuItem("Assets/Create/GameFlow/Condition...", false, 3101)]
static void Assets_CreateCondition() {
	AssetsMenuItems.CreateCondition();
}

[MenuItem("Assets/Create/GameFlow/Macro", false)]
static void Assets_Macro() {
	AssetsMenuItems.CreateMacro();
}

// GameObject > GameFlow

[MenuItem("GameObject/GameFlow/Program", false, 24)]
static void GameObject_CreateProgram() {
	GameObjectMenuItems.CreateProgram();
}

[MenuItem("GameObject/GameFlow/State Machine", false, 24)]
static void GameObject_CreateStateMachine() {
	GameObjectMenuItems.CreateStateMachine();
}

[MenuItem("GameObject/GameFlow/Timer", false, 24)]
static void GameObject_CreateTimer() {
	GameObjectMenuItems.CreateTimer();
}

[MenuItem("GameObject/GameFlow/Path", false, 24)]
public static void GameObject_CreatePath() {
	GameObjectMenuItems.CreatePath();
}

[MenuItem("GameObject/GameFlow/Pool", false, 24)]
public static void GameObject_CreatePool() {
	GameObjectMenuItems.CreatePool();
}

// Tools > GameFlow > Create

[MenuItem("Tools/GameFlow/Create/Action...", false)]
static void Tools_CreateAction() {
	AssetsMenuItems.CreateAction();
}

[MenuItem("Tools/GameFlow/Create/Condition...", false)]
static void Tools_CreateCondition() {
	AssetsMenuItems.CreateCondition();
}

[MenuItem("Tools/GameFlow/Create/Macro", false)]
static void Tools_Macro() {
	AssetsMenuItems.CreateMacro();
}

// Tools > GameFlow > Macros

[MenuItem("Tools/GameFlow/Macros/Macro Key 1 &1", false, 2001)]
static void ShortcutKey1() {
	MacroMenuItems.ShortcutKey1();
}

[MenuItem("Tools/GameFlow/Macros/Macro Key 2 &2", false, 2002)]
static void ShortcutKey2() {
	MacroMenuItems.ShortcutKey2();
}

[MenuItem("Tools/GameFlow/Macros/Macro Key 3 &3", false, 2003)]
static void ShortcutKey3() {
	MacroMenuItems.ShortcutKey3();
}

[MenuItem("Tools/GameFlow/Macros/Macro Key 4 &4", false, 2004)]
static void ShortcutKey4() {
	MacroMenuItems.ShortcutKey4();
}

[MenuItem("Tools/GameFlow/Macros/Macro Key 5 &5", false, 2005)]
static void ShortcutKey5() {
	MacroMenuItems.ShortcutKey5();
}

[MenuItem("Tools/GameFlow/Macros/Macro Key 6 &6", false, 2006)]
static void ShortcutKey6() {
	MacroMenuItems.ShortcutKey6();
}

[MenuItem("Tools/GameFlow/Macros/Macro Key 7 &7", false, 2007)]
static void ShortcutKey7() {
	MacroMenuItems.ShortcutKey7();
}

[MenuItem("Tools/GameFlow/Macros/Macro Key 8 &8", false, 2008)]
static void ShortcutKey8() {
	MacroMenuItems.ShortcutKey8();
}

[MenuItem("Tools/GameFlow/Macros/Macro Key 9 &9", false, 2009)]
static void ShortcutKey9() {
	MacroMenuItems.ShortcutKey9();
}

[MenuItem("Tools/GameFlow/Macros/Last Macro &0", false, 2020)]
static void LastMacro() {
	MacroMenuItems.LastMacro();
}

// Tools > GameFlow

[MenuItem("Tools/GameFlow/Quick Start", false, 2030)]
static void QuickStart() {
	Application.OpenURL("http://evasiongames.com/gameflow/docs/start");
}

[MenuItem("Tools/GameFlow/Release Notes ", false, 2031)]
static void ReleaseNotes() {
	Application.OpenURL("http://evasiongames.com/gameflow/releases/v1.0");
}

[MenuItem("Tools/GameFlow/About", false, 2032)]
static void About() {
	AboutWindow.Open();
}

// Window > GameFlow

[MenuItem("Window/GameFlow/Add Block... %&g", true)]
static bool ValidateAddBlock() {
	return WindowMenuItems.ValidateAddBlock();
}

[MenuItem("Window/GameFlow/Add Block... %&g")]
static void AddBlock() {
	WindowMenuItems.AddBlock();
}

}

}