// [!] DO NOT REMOVE THIS FILE

using UnityEngine;
using UnityEngine.Scripting;

namespace GameFlow {

[Preserve]
public class RuntimeReflector {

[RuntimeInitializeOnLoadMethod]
static void Init() {
	#if NETFX_CORE
	RuntimeReflection.reflector = new DotNetReflector();
	#else
	RuntimeReflection.reflector = new DefaultReflector();
	#endif
}

}

}

// ------------------------------------------------------------------------------------------------

#if NETFX_CORE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;

namespace GameFlow {

[Preserve]
public class DotNetReflector : IReflector {

private static Assembly[] _assemblies;

public Assembly[] GetAssemblies() {
    if (_assemblies != null) {
        return _assemblies;
    }
    StorageFolder folder = Package.Current.InstalledLocation;
	var task = folder.GetFilesAsync().AsTask();
    List<Assembly> list = new List<Assembly>();
    foreach (StorageFile file in task.Result) {
        if (file.FileType == ".dll" || file.FileType == ".exe") {
            try {
                var filename = file.Name.Substring(0, file.Name.Length - file.FileType.Length);
                AssemblyName name = new AssemblyName { Name = filename };
                Assembly assembly = Assembly.Load(name);
                list.Add(assembly);
            } catch {
            }
        }
    }
	_assemblies = list.ToArray();
    return _assemblies;	
}

public bool IsTypeArray(Type type) {
    return type.GetTypeInfo().IsArray;
}

public bool IsTypePrimitive(Type type) {
    return type.GetTypeInfo().IsPrimitive;
}

public bool IsTypeVisible(Type type) {
    return type.GetTypeInfo().IsVisible;
}

public bool IsTypeEnum(Type type) {
    return type.GetTypeInfo().IsEnum;
}

public bool IsTypeValueType(Type type) {
    return type.GetTypeInfo().IsValueType;
}

public bool IsTypeSubclassOf(Type type1, Type type2) {
    return type1.GetTypeInfo().IsSubclassOf(type2);
}

public bool IsTypeAssignableFrom(Type type1, Type type2) {
    return type1.GetTypeInfo().IsAssignableFrom(type2.GetTypeInfo());
}

public PropertyInfo GetTypeProperty(Type type, string name) {
	return type.GetRuntimeProperty(name);
}

public PropertyInfo[] GetTypeProperties(Type type) {
	return type.GetRuntimeProperties().ToArray();
}

public FieldInfo GetTypeField(Type type, string name) {
	return type.GetRuntimeField(name);
}

public FieldInfo[] GetTypeFields(Type type) {
	return type.GetRuntimeFields().ToArray();
}

public MethodInfo GetTypeMethod(Type type, string name, Type[] paramTypes) {
	return type.GetRuntimeMethod(name, paramTypes);
}

public MethodInfo[] GetTypeMethods(Type type) {
	return type.GetRuntimeMethods().ToArray();
}

}

}

#endif