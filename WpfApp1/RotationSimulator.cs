﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Threading;
using ModelowanieGeometryczne.ViewModel;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using WpfApp1;
using Cursor = WpfApp1.Cursor;


public class RotationSimulator : ViewModelBase
{
    //  private Stopwatch stopWatch;
    public RotationSimulator()
    {

    }

    private double _simulationTime = 10;

    public double SimulationTime
    {

        get { return _simulationTime; }
        set
        {
            _simulationTime = value;
            OnPropertyChanged(nameof(SimulationTime));
        }
    }


    private int _framesNumber = 10;

    public int FramesNumber
    {

        get { return _framesNumber; }
        set
        {
            _framesNumber = value;
            OnPropertyChanged(nameof(FramesNumber));
        }
    }


    private bool _startCordinateSystem = true;

    public bool StartCordinateSystem
    {
        get { return _startCordinateSystem; }
        set
        {
            _startCordinateSystem = true;
            OnPropertyChanged(nameof(StartCordinateSystem));
            OnPropertyChanged(nameof(FinishCordinateSystem));
            OnPropertyChanged(nameof(TempCursor));
        }
    }

    public bool FinishCordinateSystem
    {
        get { return !_startCordinateSystem; }
        set
        {
            _startCordinateSystem = false;
            OnPropertyChanged(nameof(StartCordinateSystem));
            OnPropertyChanged(nameof(FinishCordinateSystem));
            OnPropertyChanged(nameof(TempCursor));
        }
    }
  
    public Cursor TempCursor
    {
        get
        {
            if (_startCordinateSystem)
            {
                return Cursor0;
            }
            else
            {
                return Cursor1;
            }

        }
        set
        {
            if (_startCordinateSystem)
            {
                Cursor0 = value;
            }
            else
            {
                Cursor1=value;
            }
            
        }
    }

    public Cursor Cursor0{get; set;}=new Cursor();
    public Cursor Cursor1{ get; set; } = new Cursor();



    private int _animationSpeed = 20;

    public int AnimationSpeed
    {
        get { return _animationSpeed; }
        set { _animationSpeed = value; }
    }

   
    public void OnLoad()
    {

    }

    public void OnUpdateFrame()
    {
        //Material1.OnUpdateFrame();
        //Cutter1.OnUpdateFrame();

    }

    private double counter = 0;
    private double scale = 0.01;

    public double Scale
    {
        get => scale;
        set => scale = Math.Max(value, 0.0001);
    }

    private Vector3? frozenCutterCeneterPointPosition;

    public void OnRenderFrame(double alphaX, double alphaY, double alphaZ)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.PushAttrib(AttribMask.LightingBit);
        GL.PushAttrib(AttribMask.LightingBit);
        GL.Disable(EnableCap.Lighting);
        GL.Color3(Color.White);
        GL.PopAttrib();
        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadIdentity();
        var modelview = Matrix4.LookAt(0.0f, 3.5f, 3.5f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f);
        GL.LoadMatrix(ref modelview);

        //counter += e.Time;
        // counter += 0.1;
        //  GL.Rotate(counter * 10, 0, 1, 0);
        GL.Rotate(alphaX, 1, 0, 0);
        GL.Rotate(alphaY, 0, 1, 0);
        GL.Rotate(alphaX, 0, 0, 1);
        GL.Scale(Scale, Scale, Scale);
        GL.Rotate(50, 0, 1, 0);

        Cursor0.Draw();
        Cursor1.Draw();
        CurrentCursor.Draw();

        foreach (var item in AnimationFramesList)
        {
            item.Draw();
        }

    }

   
    
    

    private DispatcherTimer timer;
    int _stepNumber = 0;

    public int StepNumber
    {
        get
        {
            return _stepNumber;
        }
        set
        {
            _stepNumber = value;
            OnPropertyChanged(nameof(StepNumber));
        }
    }

    private bool _simulationResultButtonIsEnabled=true;

    public bool SimulationResultButtonIsEnabled
    {
        get { return _simulationResultButtonIsEnabled; }
        set
        {
            _simulationResultButtonIsEnabled = value;
            OnPropertyChanged(nameof(SimulationResultButtonIsEnabled));
        }
    }

    private bool _simulationStartButtonIsEnabled = true;

    public bool SimulationStartButtonIsEnabled
    {
        get { return _simulationStartButtonIsEnabled; }
        set
        {
            _simulationStartButtonIsEnabled = value;
            OnPropertyChanged(nameof(SimulationStartButtonIsEnabled));
        }
    }

    private bool _loadPathButtonIsEnabled = true;

    public bool LoadPathButtonIsEnabled
    {
        get { return _loadPathButtonIsEnabled; }
        set
        {
            _loadPathButtonIsEnabled = value;
            OnPropertyChanged(nameof(LoadPathButtonIsEnabled));
        }
    }
    public void StartSimulation()
    {

        SimulationResultButtonIsEnabled = false;
        SimulationStartButtonIsEnabled = false;
        LoadPathButtonIsEnabled = false;
        StepNumber = 0;
        GenerateAnimationFrames(_simulationTime, _framesNumber);
        timer = new DispatcherTimer();
        //timer.Interval = TimeSpan.FromMilliseconds(30);
        timer.Interval = TimeSpan.FromSeconds(_simulationTime/(_framesNumber-1));
        timer.Tick += TimerOnTick;
        timer.Start();

    }

    private Cursor CurrentCursor =new Cursor();
    private List<Cursor> AnimationFramesList=new List<Cursor>();
    public void GenerateAnimationFrames(double simulationTime, int framesNumber)
    {
        AnimationFramesList.Clear();
        for (int i = 0; i <= framesNumber; i++)
        {
            var temp =new Cursor();
            double a = ((double) (framesNumber - i) / framesNumber);
            double b = ((double) i / framesNumber);
            temp.EulerAngles = a* Cursor0.EulerAngles +  b* Cursor1.EulerAngles;
            temp.Origin = a* Cursor0.Origin +  b* Cursor1.Origin;
            
            AnimationFramesList.Add(temp);
        }
    }


    private void TimerOnTick(object sender, EventArgs e)
    {
        if(StepNumber>(AnimationFramesList.Count-1))
        {
            timer.Stop();
        }
        else
        {
            CurrentCursor = AnimationFramesList[StepNumber];
            Refresh();
        }

        StepNumber++;

    }

    private bool _showPath=false;
    public bool ShowPath
    {
        get { return _showPath; }
        set
        {
            _showPath = value;
            OnPropertyChanged(nameof(ShowPath));
            Refresh();
        }
    }

    private bool stopSimulationFlag = false;
    public void StopSimulation()
    {
        stopSimulationFlag = true;

    }



}