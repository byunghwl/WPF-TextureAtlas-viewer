using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TextureAtlas
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// //https://codeincomplete.com/posts/bin-packing/
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Texture> _textures = new List<Texture>();

        Bin             _rootBin;
        int             _index = 0;
        DispatcherTimer _timer;

        //-----------------------------------------
        public class Texture
        {
            public int Width;
            public int Height;
            public Color Color;

            public Texture(int width, int height, Color color  )
            {
                Width = width; Height = height; Color = color;
            }
        }

        //-----------------------------------------
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        //-----------------------------------------
        private void MainWindow_Loaded( object sender, RoutedEventArgs e )
        {
            CreateTestData();
            ShowTextureAtlas();
        }

        //-----------------------------------------
        class Bin
        {
            public int  Height { get; private set; }
            public int  Width { get; private set; }

            public bool IsOccupied { get; private set; }

            public Bin  Right;
            public Bin  Down;

            //-----------------------------------------
            public Bin( int width, int height)
            {
                Height = height;
                Width  = width;
                Right = Down = null;
            }

            //-----------------------------------------
            public bool FindBin( Texture texture, Canvas canvas )
            {
                Bin resultBin = FindBin( this, texture, canvas, 0, 0 );
                if( resultBin == null )
                {
                    // need to grow box
                    int totalWidth  = Convert.ToInt32( canvas.ActualWidth );
                    int totalHeight = Convert.ToInt32( canvas.ActualHeight );

                    bool betterToGrowDown     = texture.Width  >= totalWidth / 2 ;
                    bool betterToGrowRight    = texture.Height >= totalHeight / 2 ;

                    if( betterToGrowRight && betterToGrowDown )
                    {
                        betterToGrowRight = ( texture.Width < texture.Height );
                        betterToGrowDown = !betterToGrowRight;
                    }

                    if( betterToGrowRight )
                    {
                        int x = 0;
                        Bin farRightbin = FindFarRightBin( this, ref x );
                        farRightbin.Right = new Bin( texture.Width, totalHeight );

                        resultBin = FindBin( farRightbin.Right, texture, canvas, x, 0  );
                    }
                    else if( betterToGrowDown )
                    {
                        int y = 0;
                        Bin veryBottombin = FindVeryBottomBin( this, ref y );
                        veryBottombin.Down = new Bin( totalWidth, texture.Height );

                        resultBin = FindBin( veryBottombin.Down, texture, canvas, 0, y );
                    }

                }
                return ( resultBin != null );
            }

            //-----------------------------------------
            private static Bin FindFarRightBin( Bin bin, ref int x )
            {
                x += bin.Width;

                // better to merge?
                if ( bin.Right == null ) {
                    if( !bin.IsOccupied ) {
                        x -= bin.Width;
                        bin.Width = 0;
                    }
                    return bin;
                }
                return FindFarRightBin( bin.Right, ref x );
            }

            //-----------------------------------------
            private static Bin FindVeryBottomBin( Bin bin, ref int y )
            {
                y += bin.Height;

                if ( bin.Down == null ) {
                    // better to merge?
                    if ( !bin.IsOccupied ) {
                        y -= bin.Height;
                        bin.Height = 0;
                    }
                    return bin;
                }
                return FindVeryBottomBin( bin.Down, ref y );
            }

            //-----------------------------------------
            private static Bin FindBin( Bin bin, Texture texture, Canvas canvas, int x, int y )
            {
                if ( bin == null ) return null;

                Bin resultBin = null;

                // check this bin fits for texture
                if ( !bin.IsOccupied && 
                    texture.Width <= bin.Width && 
                    texture.Height <= bin.Height )
                {
                    bin.IsOccupied = true;
                    CreateRectangle( canvas, texture, y, x );

                    int remainWidth  = bin.Width  - texture.Width;
                    int remainHeight = bin.Height - texture.Height;

                    if ( remainWidth > 0 ) {
                        bin.Right = new Bin( remainWidth, texture.Height );
                    }

                    if ( remainHeight > 0 ) {
                        bin.Down = new Bin( bin.Width, remainHeight );
                    }

                    bin.Height = texture.Height;
                    bin.Width  = texture.Width;

                    resultBin = bin;
                }

                if ( resultBin != null ){
                    return resultBin;
                } else {
                    resultBin = FindBin( bin.Right, texture, canvas, x + bin.Width, y);
                    if ( resultBin != null ) return resultBin;
                    resultBin = FindBin( bin.Down, texture, canvas, x, y + bin.Height );
                    return resultBin;
                }
            }
        }

        //-----------------------------------------
        private void ShowTextureAtlas()
        {
            double totalWidth = ThisCanvas.ActualWidth;
            double totalHeight = ThisCanvas.ActualHeight;

            _rootBin = new Bin( Convert.ToInt32( totalWidth ), Convert.ToInt32( totalHeight ) );

            _timer = new DispatcherTimer();
            _timer.Tick += new EventHandler(Tick);
            _timer.Interval = new TimeSpan( 0, 0, 0,0, 100 );
            
            _timer.Start();
        }

        //-----------------------------------------
        void Tick( object sender, EventArgs e )
        {
            if( _index < _textures.Count )
                _rootBin.FindBin( _textures[ _index++ ], ThisCanvas );
            else {
                _timer.Stop();
            }
        }

        //-----------------------------------------
        private void CreateTestData()
        {
            _textures.Add( new Texture( 100, 100, ColorFromRGB(255,0,0) ) );
            _textures.Add( new Texture( 400, 100, ColorFromRGB(0,255,0) ) );
            _textures.Add( new Texture( 100, 300, ColorFromRGB( 0, 0, 255 ) ) );
            _textures.Add( new Texture( 100, 100, ColorFromRGB( 255, 255, 0 ) ) );
            _textures.Add( new Texture( 100, 100, ColorFromRGB( 255, 0, 255 ) ) );
            _textures.Add( new Texture( 300, 200, ColorFromRGB( 55, 255, 255 ) ) );
            _textures.Add( new Texture( 700, 200, ColorFromRGB( 0, 0, 0) ) );
            _textures.Add( new Texture( 200, 100, ColorFromRGB( 255, 120, 255 ) ) );
            _textures.Add( new Texture( 300, 100, ColorFromRGB( 255, 10, 120 ) ) );
            _textures.Add( new Texture( 300, 100, ColorFromRGB( 255, 10, 120 ) ) );
            _textures.Add( new Texture( 30, 30, ColorFromRGB( 255, 10, 120 ) ) );
            _textures.Add( new Texture( 300, 100, ColorFromRGB( 20, 10, 120 ) ) );
            _textures.Add( new Texture( 100, 50, ColorFromRGB( 128, 10, 120 ) ) );
            _textures.Add( new Texture( 50, 70, ColorFromRGB( 50, 60, 120 ) ) );
        }

        //-----------------------------------------
        private static Color ColorFromRGB( int r, int g, int b )
        {
            return Color.FromRgb( ( byte )r, ( byte )g, ( byte )b );
        }
        
        //-----------------------------------------
        private static void CreateRectangle( Canvas canvas, Texture texture ,int top, int left )
        {
            CreateRectangle( canvas, top, left, texture.Width, texture.Height, new SolidColorBrush( texture.Color ) );
        }

        //-----------------------------------------
        private static void CreateRectangle( Canvas canvas, int top, int left, int width, int height, SolidColorBrush colorBrush )
        {
            Rectangle rect = new Rectangle();
            rect.Width  = width;
            rect.Height = height;
            rect.Fill   = colorBrush;

            Canvas.SetTop( rect, top );
            Canvas.SetLeft( rect, left );

            canvas.Children.Add( rect );
        }


    }
}
