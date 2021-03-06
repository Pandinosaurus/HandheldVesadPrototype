﻿using NormandErwan.MasterThesis.Experiment.Experiment.Task;
using NormandErwan.MasterThesis.Experiment.Inputs.Interactables;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NormandErwan.MasterThesis.Experiment.Loggers
{
  public class ExperimentDetailsLogger : ExperimentBaseLogger
  {
    // Variables

    protected bool itemSelected, itemDeselected, itemMoved, itemClassified;
    protected Container selectedContainer;
    protected Item selectedItem;
    protected bool panningActivated, panning, zoomingActivated, zooming;
    protected Vector3 panningTranslation, zoomingTranslation;
    protected Vector3 zoomingScaling;

    // MonoBehaviour methods

    protected override void Awake()
    {
      base.Awake();

      ResetTaskGridEvents();
      ResetPanning();
      ResetZooming();

      selectedItem = null;
      selectedContainer = null;
      panningActivated = zoomingActivated = false;
    }

    protected void LateUpdate()
    {
      if (IsConfigured && stateController.CurrentState == stateController.taskTrialState)
      {
        if (itemDeselected && !itemSelected && !itemMoved)
        {
          selectedItem = null;
          selectedContainer = null;
        }

        PrepareRow();

        AddToRow(Time.frameCount);
        AddToRow(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"));

        AddToRow(deviceController.ParticipantId);
        AddToRow(technique.CurrentCondition.Id);
        AddToRow(technique.CurrentCondition.Name);
        AddToRow(textSize.CurrentCondition.Id);
        AddToRow(textSize.CurrentCondition.Name);
        AddToRow(distance.CurrentCondition.Id);
        AddToRow(distance.CurrentCondition.Name);
        AddToRow(stateController.CurrentTrial);

        AddToRow(taskGrid.transform, false);
        AddToRow(taskGrid.LossyScale);
        AddToRow(taskGrid.IsConfigured);
        AddToRow(taskGrid.IsCompleted);
        AddToRow((int)taskGrid.Mode);
        AddToRow(GetTaskGridModeName());

        AddToRow(panningActivated);
        AddToRow(panning);
        AddToRow(panningTranslation);

        AddToRow(zoomingActivated);
        AddToRow(zooming);
        AddToRow(zoomingScaling);
        AddToRow(zoomingTranslation);

        AddToRow(itemSelected);
        AddToRow(itemDeselected);
        AddToRow(itemMoved);
        AddToRow(itemClassified);
        AddToRow(selectedContainer);
        AddToRow(selectedItem);

        AddToRow(Index.IsVisible);
        AddToRow(Index.IsTracked);
        AddToRow(Index.transform, false);

        AddToRow(Thumb.IsTracked);
        AddToRow(Thumb.transform, false);

        AddToRow(ProjectedIndex.IsOnGrid);
        AddToRow(ProjectedIndex.transform, false);

        AddToRow(ProjectedThumb.IsOnGrid);
        AddToRow(ProjectedThumb.transform, false);

        AddToRow(head, false);
        AddToRow(mobileDevice.IsTracking);
        AddToRow(mobileDevice.transform, false);

        WriteRow();

        ResetTaskGridEvents();
        ResetPanning();
        ResetZooming();
      }
    }

    // Methods

    public override void Configure()
    {
      Filename = "participant-" + deviceController.ParticipantId + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_details.csv";

      Columns = new List<string>() {
        "frame_id", "datetime",
        "participant_id",
        "technique_id", "technique_name",
        "text_size_id", "text_size_name",
        "distance_id", "distance_name",
        "trial"
      };

      AddTransformToColumns("grid");
      Columns.AddRange(new string[] {
        "grid_is_configured", "grid_is_completed",
        "grid_mode", "grid_mode_name"
      });

      Columns.AddRange(new string[] { "panningActivated", "panning" });
      AddVector3ToColumns("panning_translation");

      Columns.AddRange(new string[] { "zoomingActivated", "zooming" });
      AddVector3ToColumns("zooming_scaling");
      AddVector3ToColumns("zooming_translation");

      Columns.AddRange(new string[] {
        "item_selected", "item_deselected",
        "item_moved", "item_classified",
        "selected_container", "selected_item"
      });

      Columns.Add("index_active");
      Columns.Add("index_tracked");
      AddTransformToColumns("index", false);

      Columns.Add("thumb_tracked");
      AddTransformToColumns("thumb", false);

      Columns.Add("projected_index_active");
      AddTransformToColumns("projected_index", false);

      Columns.Add("projected_thumb_active");
      AddTransformToColumns("projected_thumb", false);

      AddTransformToColumns("head", false);

      Columns.Add("phone_tracked");
      AddTransformToColumns("phone", false);

      base.Configure();
    }

    protected override void TaskGrid_ItemSelected(Container container, Item item)
    {
      if (item.IsSelected)
      {
        itemSelected = true;
        selectedContainer = container;
        selectedItem = item;
      }
      else
      {
        itemDeselected = true;
      }
    }

    protected override void TaskGrid_ItemMoved(Container oldContainer, Container newContainer, Item item, TaskGrid.ItemMovedType moveType)
    {
      itemMoved = true;
      if (moveType == TaskGrid.ItemMovedType.Classified)
      {
        itemClassified = true;
      }
      selectedContainer = newContainer;
      selectedItem = item;
    }

    protected override void TaskGrid_DraggingStarted(IDraggable grid)
    {
      panningActivated = true;
    }

    protected override void TaskGrid_Dragging(IDraggable grid, Vector3 translation)
    {
      panning = true;
      panningTranslation += translation;
    }

    protected override void TaskGrid_DraggingStopped(IDraggable grid)
    {
      panningActivated = false;
    }

    protected override void TaskGrid_ZoomingStarted(IZoomable grid)
    {
      zoomingActivated = true;
    }

    protected override void TaskGrid_Zooming(IZoomable grid, Vector3 scaling, Vector3 translation)
    {
      zooming = true;
      zoomingScaling = Vector3.Scale(zoomingScaling, scaling);
      zoomingTranslation += translation;
    }

    protected override void TaskGrid_ZoomingStopped(IZoomable grid)
    {
      zoomingActivated = false;
    }

    protected void AddToRow(Container container)
    {
      if (container == null)
      {
        AddToRow("");
      }
      else
      {
        var position = taskGrid.GetPosition(container);
        AddToRow("(" + position.x + ", " + position.y + ")");
      }
    }

    protected void AddToRow(Item item)
    {
      AddToRow((item == null) ? "" : selectedContainer.Elements.IndexOf(item).ToString());
    }

    protected string GetTaskGridModeName()
    {
      switch (taskGrid.Mode)
      {
        case TaskGrid.InteractionMode.Select: return "select";
        case TaskGrid.InteractionMode.Pan: return "pan";
        case TaskGrid.InteractionMode.Zoom: return "zoom";
        default: return "all"; // Possibilities are only one mode activated or all of them
      }
    }

    protected void ResetTaskGridEvents()
    {
      if (itemMoved || itemClassified)
      {
        selectedItem = null;
        selectedContainer = null;
      }
      itemSelected = itemDeselected = itemMoved = itemClassified = false;
    }

    protected void ResetPanning()
    {
      if (panning)
      {
        panning = false;
        panningTranslation = Vector3.zero;
      }
    }

    protected void ResetZooming()
    {
      if (zooming)
      {
        zooming = false;
        zoomingScaling = Vector3.one;
        zoomingTranslation = Vector3.zero;
      }
    }
  }
}