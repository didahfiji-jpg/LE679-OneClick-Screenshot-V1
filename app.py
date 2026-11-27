import ctypes as c
from ctypes import wintypes as w
import tkinter as tk
from pathlib import Path
from datetime import datetime
from PIL import Image, ImageGrab, ImageTk
import io
import win32clipboard
u=c.windll.user32
u.SetProcessDPIAware()
u.GetForegroundWindow.restype=w.HWND
root=tk.Tk()
root.title("LE679 OneClick Screenshot V1")
root.overrideredirect(True)
root.attributes("-topmost",True)
root.configure(bg="#010203")
root.attributes("-transparentcolor","#010203")
root.geometry("80x80+40+200")
import sys
icon=Image.open(Path(getattr(sys,"_MEIPASS",Path(__file__).parent))/"icon.png")
photo=ImageTk.PhotoImage(icon.resize((80,80),Image.Resampling.LANCZOS))
root.iconphoto(True,photo)
label=tk.Label(root,image=photo,bg="#010203",bd=0)
label.pack()
root.update_idletasks()
hwnd=u.GetParent(root.winfo_id()) or root.winfo_id()
u.GetWindowLongW.argtypes=[w.HWND,c.c_int]
u.SetWindowLongW.argtypes=[w.HWND,c.c_int,c.c_long]
u.SetWindowLongW(hwnd,-20,u.GetWindowLongW(hwnd,-20)|0x08000000|0x80)
def down(e):
    global start,target,origin,drag
    start=(e.x_root,e.y_root)
    origin=(root.winfo_x(),root.winfo_y())
    target=u.GetForegroundWindow()
    drag=False
def move(e):
    global drag
    dx,dy=e.x_root-start[0],e.y_root-start[1]
    if abs(dx)+abs(dy)>5: drag=True
    if drag: root.geometry(f"80x80{origin[0]+dx:+d}{origin[1]+dy:+d}")
def capture():
    try:
        r=w.RECT()
        if not target or target==hwnd or not u.GetWindowRect(target,c.byref(r)): return
        if c.windll.dwmapi.DwmGetWindowAttribute(target,9,c.byref(r),c.sizeof(r))!=0:
            u.GetWindowRect(target,c.byref(r))
        if r.right<=r.left or r.bottom<=r.top: return
        im=ImageGrab.grab(bbox=(r.left,r.top,r.right,r.bottom),all_screens=True).convert("RGB")
        folder=Path.home()/"Pictures"/"LE679 Screenshots"
        folder.mkdir(parents=True,exist_ok=True)
        im.save(folder/(datetime.now().strftime("%Y%m%d_%H%M%S_%f")+".png"))
        b=io.BytesIO(); im.save(b,"BMP")
        win32clipboard.OpenClipboard()
        try:
            win32clipboard.EmptyClipboard()
            win32clipboard.SetClipboardData(win32clipboard.CF_DIB,b.getvalue()[14:])
        finally: win32clipboard.CloseClipboard()
    finally:
        root.deiconify()
def up(e):
    if not drag:
        root.withdraw()
        root.after(150,capture)
label.bind("<ButtonPress-1>",down)
label.bind("<B1-Motion>",move)
label.bind("<ButtonRelease-1>",up)
root.mainloop()
