; AutoHotkey v2 script for deterministic PowerScribe paste
#Requires AutoHotkey v2.0+

PasteToPowerScribe(pasteText, expectedHash := "") {
    if (!WinExist("ahk_exe powerscribe.exe")) {
        MsgBox "PowerScribe is not focused."
        return false
    }

    ClipBackup := A_Clipboard
    A_Clipboard := pasteText
    ClipWait 0.5

    if (expectedHash) {
        hash := StrLen(A_Clipboard) . "::" . CRC32(A_Clipboard)
        if (hash != expectedHash) {
            A_Clipboard := ClipBackup
            MsgBox "Clipboard verification failed."
            return false
        }
    }

    Send "^v"
    Sleep 150
    Send "{Tab}"
    A_Clipboard := ClipBackup
    return true
}

CRC32(str) {
    static nt := DllCall("GetModuleHandle", "Str", "Ntdll.dll", "Ptr")
    buffer := Buffer(StrLen(str))
    StrPut(str, buffer, "UTF-8")
    h := DllCall("ntdll\!RtlComputeCrc32", "UInt", 0, "Ptr", buffer.Ptr, "UInt", buffer.Size, "UInt")
    return Format("{1:08X}", h)
}
