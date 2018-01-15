﻿using NormandErwan.MasterThesis.Experiment.Experiment.Task;
using NormandErwan.MasterThesis.Experiment.Experiment.Variables;
using NormandErwan.MasterThesis.Experiment.Inputs.Interactables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace NormandErwan.MasterThesis.Experiment.Loggers
{
  public class ParticipantTrialsLogger : ExperimentBaseLogger
  {
    public class Variable
    {
      public string name;
      public int count;
      public Stopwatch time = new Stopwatch();
      public float distance;

      public Variable(string name)
      {
        this.name = name;
        Reset();
      }

      public void Reset()
      {
        count = 0;
        time.Reset();
        distance = 0;
      }

      public List<string> Columns()
      {
        return new List<string>() { name + "_count", name + "_time", name + "_distance" };
      }
    }

    // Variables

    protected DateTime startDateTime;

    protected Variable selections = new Variable("selections");
    protected int deselections = 0;
    protected int errors = 0;
    protected int classifications = 0;

    protected Variable pan = new Variable("pan");
    protected Variable zoom = new Variable("zoom");

    protected float headPhoneDistance = 0;
    protected float oldHeadPhoneDistance = 0;

    // MonoBehaviour methods

    protected virtual void Start()
    {
      oldHeadPhoneDistance = (head.position - mobileDevice.position).magnitude;
    }

    protected virtual void LateUpdate()
    {
      float headPhoneDistance = (head.position - mobileDevice.position).magnitude;
      this.headPhoneDistance += headPhoneDistance - oldHeadPhoneDistance;
      oldHeadPhoneDistance = headPhoneDistance;
    }

    // Methods

    public override void Configure()
    {
      Filename = "participant-" + deviceController.ParticipantId + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_trials.csv";

      Columns = new List<string>() {
        "participant_id", "technique", "distance", "text_size", "trial_number", "grid_config",
        "start_date_time", "total_time"
      };
      Columns.AddRange(selections.Columns());
      Columns.AddRange(new string[]{ "deselections", "errors", "classifications" });
      Columns.AddRange(pan.Columns());
      Columns.AddRange(zoom.Columns());
      Columns.Add("head_phone_distance");

      base.Configure();
    }

    protected override void Grid_Configured()
    {
      if (stateController.CurrentState.id == stateController.taskTrialState.id)
      {
        PrepareRow();

        startDateTime = DateTime.Now;

        selections.Reset();
        deselections = 0;
        errors = 0;

        pan.Reset();
        zoom.Reset();

        AddToRow(deviceController.ParticipantId);
        AddToRow(stateController.GetIndependentVariable<IVTechnique>().CurrentCondition.id);
        AddToRow(stateController.GetIndependentVariable<IVClassificationDifficulty>().CurrentCondition.id);
        AddToRow(stateController.GetIndependentVariable<IVTextSize>().CurrentCondition.id);
        AddToRow(stateController.CurrentTrial);
        AddToRow(grid.GridGenerator.ToString());
      }
    }

    protected override void Grid_Completed()
    {
      if (stateController.CurrentState.id == stateController.taskTrialState.id)
      {
        AddToRow(startDateTime);
        AddToRow((DateTime.Now - startDateTime).TotalSeconds);

        AddToRow(selections);
        AddToRow(deselections);
        AddToRow(errors);
        AddToRow(classifications);

        AddToRow(pan);
        AddToRow(zoom);

        AddToRow(headPhoneDistance);

        WriteRow();
      }
    }

    protected override void Grid_ItemSelected(Container container, Item item)
    {
      if (item.IsSelected)
      {
        selections.count++;
        selections.time.Start();
      }
      else
      {
        deselections++;
        selections.time.Stop();
      }
    }

    protected override void Grid_ItemMoved(Container oldContainer, Container newContainer, Item item, Experiment.Task.Grid.ItemMovedType moveType)
    {
      if (moveType == Experiment.Task.Grid.ItemMovedType.Classified)
      {
        classifications++;
      }
      else if (moveType == Experiment.Task.Grid.ItemMovedType.Error)
      {
        errors++;
      }
      selections.time.Stop();
    }

    protected override void Grid_DraggingStarted(IDraggable grid)
    {
      pan.count++;
      pan.time.Start();
    }

    protected override void Grid_Dragging(IDraggable grid, Vector3 translation)
    {
      var magnitude = translation.magnitude;
      pan.distance += magnitude;

      if (selections.time.IsRunning)
      {
        selections.distance += magnitude;
      }
    }

    protected override void Grid_DraggingStopped(IDraggable grid)
    {
      pan.time.Stop();
    }

    protected override void Grid_ZoomingStarted(IZoomable grid)
    {
      zoom.count++;
      zoom.time.Start();
    }

    protected override void Grid_Zooming(IZoomable grid, float scaleFactor, Vector3 translation, Vector3[] cursors)
    {
      var distance = cursors[0] - cursors[1];
      var previousDistance = cursors[2] - cursors[3];
      var magnitude = (distance - previousDistance).magnitude;

      zoom.distance += magnitude;

      if (selections.time.IsRunning)
      {
        selections.distance += magnitude;
      }
    }

    protected override void Grid_ZoomingStopped(IZoomable grid)
    {
      zoom.time.Stop();
    }

    protected virtual void AddToRow(Variable variable)
    {
      AddToRow(variable.count);
      AddToRow(variable.time.Elapsed.TotalSeconds);
      AddToRow(variable.distance);
    }
  }
}