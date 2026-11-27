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
hdIcon=new Bitmap(256,256,PixelFormat.Format32bppArgb);hdIcon.SetResolution(300,300);using(var g=Graphics.FromImage(hdIcon)){g.Clear(Color.Transparent);DrawIcon(g,256);}DoubleBuffered=true;
var region=new System.Drawing.Drawing2D.GraphicsPath();region.AddEllipse(0,0,79,79);Region=new Region(region);
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
protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;g.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;g.DrawImage(hdIcon,new Rectangle(0,0,ClientSize.Width,ClientSize.Height),0,0,256,256,GraphicsUnit.Pixel);}
protected override void Dispose(bool disposing){if(disposing && hdIcon!=null){hdIcon.Dispose();hdIcon=null;}base.Dispose(disposing);}
static void DrawIcon(Graphics g,int size){
g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;g.ScaleTransform(size/80f,size/80f);
using(var brush=new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(1,1,78,78),Color.FromArgb(151,48,223),Color.FromArgb(35,5,65),90f))g.FillEllipse(brush,1,1,78,78);
for(int i=0;i<5;i++){using(var p=new Pen(Color.FromArgb(65+i*35,190,65,255),1.4f))g.DrawEllipse(p,2+i,2+i,75-i*2,75-i*2);}
using(var p=new Pen(Color.FromArgb(170,255,220,255),1.5f))g.DrawArc(p,4,4,71,71,195,105);
using(var p=new Pen(Color.White,3.2f)){p.LineJoin=System.Drawing.Drawing2D.LineJoin.Round;g.DrawPolygon(p,new[]{new Point(20,31),new Point(30,31),new Point(34,25),new Point(46,25),new Point(50,31),new Point(60,31),new Point(60,55),new Point(20,55)});g.DrawEllipse(p,32,35,16,16);}
}
[STAThread] static void Main(){SetProcessDPIAware();Application.EnableVisualStyles();Application.Run(new Capture());}
}