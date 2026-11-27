using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
class Capture : Form {
[DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
[DllImport("user32.dll")] static extern bool SetProcessDPIAware();
[DllImport("dwmapi.dll")] static extern int DwmGetWindowAttribute(IntPtr h,int a,out RECT r,int s);
[StructLayout(LayoutKind.Sequential)] struct RECT { public int L,T,R,B; }
Bitmap hdIcon;
Point start,origin; bool dragging; IntPtr target;
protected override bool ShowWithoutActivation {get{return true;}}
protected override CreateParams CreateParams {get{var p=base.CreateParams;p.ExStyle|=0x08000000|0x80;return p;}}
Capture(){
Text="LE679 OneClick Screenshot V1"; FormBorderStyle=FormBorderStyle.None; TopMost=true; ShowInTaskbar=false; Size=new Size(80,80); BackColor=Color.FromArgb(41,24,63); StartPosition=FormStartPosition.Manual; Location=new Point(40,200);
using(var stream=typeof(Capture).Assembly.GetManifestResourceStream("LE679.Icon.png")){if(stream==null)throw new InvalidOperationException("Embedded icon missing");using(var original=new Bitmap(stream)){hdIcon=new Bitmap(original);}}DoubleBuffered=true;
SetTransparentIconShape();
var menu=new ContextMenuStrip();menu.Items.Add("Exit",null,(s,e)=>Close());ContextMenuStrip=menu;
MouseDown+=(s,e)=>{if(e.Button!=MouseButtons.Left)return;start=Cursor.Position;origin=Location;target=GetForegroundWindow();dragging=false;};
MouseMove+=(s,e)=>{if(e.Button!=MouseButtons.Left)return;var p=Cursor.Position;int x=p.X-start.X,y=p.Y-start.Y;if(Math.Abs(x)+Math.Abs(y)>5)dragging=true;if(dragging)Location=new Point(origin.X+x,origin.Y+y);};
MouseUp+=(s,e)=>{if(e.Button!=MouseButtons.Left||dragging)return;Hide();var timer=new Timer{Interval=180};timer.Tick+=(a,b)=>{timer.Stop();timer.Dispose();try{RECT r;if(target==IntPtr.Zero||DwmGetWindowAttribute(target,9,out r,16)!=0)return;int w=r.R-r.L,h=r.B-r.T;if(w<=0||h<=0)return;using(var image=new Bitmap(w,h)){using(var g=Graphics.FromImage(image))g.CopyFromScreen(r.L,r.T,0,0,image.Size);var dir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),"LE679 Screenshots");Directory.CreateDirectory(dir);image.Save(Path.Combine(dir,DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")+".png"),ImageFormat.Png);Clipboard.SetImage(image);Flash(Screen.FromHandle(target).Bounds);}}catch(Exception ex){System.Diagnostics.Trace.WriteLine(ex.Message);}finally{Show();}};timer.Start();};
}
void Flash(Rectangle bounds){
var flash=new FlashWindow();flash.Bounds=bounds;flash.Show();flash.Refresh();
var t=new Timer{Interval=100};t.Tick+=(s,e)=>{t.Stop();t.Dispose();flash.Close();flash.Dispose();};t.Start();
}
class FlashWindow:Form{
public FlashWindow(){FormBorderStyle=FormBorderStyle.None;StartPosition=FormStartPosition.Manual;TopMost=true;ShowInTaskbar=false;BackColor=Color.White;}
protected override bool ShowWithoutActivation{get{return true;}}
protected override CreateParams CreateParams{get{var p=base.CreateParams;p.ExStyle|=0x08000000|0x80|0x20;return p;}}
}
void SetTransparentIconShape(){using(var path=new System.Drawing.Drawing2D.GraphicsPath()){for(int row=0;row<80;row++){int from=-1;for(int col=0;col<=80;col++){bool solid=false;if(col<80){int px=Math.Min(hdIcon.Width-1,(int)((col+0.5)*hdIcon.Width/80));int py=Math.Min(hdIcon.Height-1,(int)((row+0.5)*hdIcon.Height/80));solid=hdIcon.GetPixel(px,py).A>8;}if(solid && from<0)from=col;if(!solid && from>=0){path.AddRectangle(new Rectangle(from,row,col-from,1));from=-1;}}}Region=new Region(path);}}
protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;g.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;g.DrawImage(hdIcon,new Rectangle(0,0,ClientSize.Width,ClientSize.Height),0,0,hdIcon.Width,hdIcon.Height,GraphicsUnit.Pixel);}
protected override void Dispose(bool disposing){if(disposing && hdIcon!=null){hdIcon.Dispose();hdIcon=null;}base.Dispose(disposing);}
[STAThread] static void Main(){SetProcessDPIAware();Application.EnableVisualStyles();Application.Run(new Capture());}
}