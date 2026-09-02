#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ClearPlayerPrefs
{
    // Tạo thêm 1 mục trên thanh Menu: Tools -> Clear All PlayerPrefs
    [MenuItem("Tools/Clear All PlayerPrefs")]
    public static void ClearAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("<color=green>SUCCESS:</color> Đã xóa toàn bộ dữ liệu PlayerPrefs thành công!");
    }
}
#endif