﻿/*
 * This file is part of alphaTab.
 * Copyright c 2013, Daniel Kuschny and Contributors, All rights reserved.
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3.0 of the License, or at your option any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library.
 */
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AlphaTab.Model;

namespace AlphaTab.Wpf.Share.ViewModel
{
    /// <summary>
    /// A viewmodel for displaying track information in the UI
    /// </summary>
    public class TrackViewModel : INotifyPropertyChanged
    {
        private TrackType _trackType;
        private bool[] _usedBars;
        private bool _isSelected;
        private Track _track;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public TrackType TrackType
        {
            get { return _trackType; }
            set
            {
                _trackType = value;
                OnPropertyChanged("TrackType");
            }
        }

        public string Name
        {
            get { return _track.Name; }
            set
            {
                _track.Name = value;
                OnPropertyChanged("Name");
            }
        }

        public int Volume
        {
            get { return _track.PlaybackInfo.Volume; }
            set
            {
                _track.PlaybackInfo.Volume = value;
                OnPropertyChanged("Volume");
            }
        }

        public bool IsSolo
        {
            get { return _track.PlaybackInfo.IsSolo; }
            set
            {
                _track.PlaybackInfo.IsSolo = value;
                OnPropertyChanged("IsSolo");
            }
        }

        public bool IsMute
        {
            get { return _track.PlaybackInfo.IsMute; }
            set
            {
                _track.PlaybackInfo.IsMute = value;
                OnPropertyChanged("IsMute");
            }
        }

        public bool[] UsedBars
        {
            get { return _usedBars; }
            set
            {
                _usedBars = value;
                OnPropertyChanged("UsedBars");
            }
        }

        public Track Track
        {
            get { return _track; }
            set
            {
                _track = value;
                OnPropertyChanged("Track");
            }
        }

        public TrackViewModel(Track track)
        {
            _track = track;

            // general midi Programs
            if (track.IsPercussion)
            {
                TrackType = TrackType.Drums;
            }
            else if (track.PlaybackInfo.Program >= 0 && track.PlaybackInfo.Program <= 6)
            {
                TrackType = TrackType.Piano;
            }
            else if (track.PlaybackInfo.Program >= 26 && track.PlaybackInfo.Program <= 31)
            {
                TrackType = TrackType.ElectricGuitar;
            }
            else if (track.PlaybackInfo.Program >= 32 && track.PlaybackInfo.Program <= 39)
            {
                TrackType = TrackType.BassGuitar;
            }
            else 
            {
                TrackType = TrackType.Default;
            }

            // scan all bars if they have any note 
            _usedBars = new bool[track.Bars.Count];
            for (int barI = 0; barI < track.Bars.Count; barI++)
            {
                Bar bar = track.Bars[barI];
                _usedBars[barI] = false;

                for (int voiceI = 0; voiceI < bar.Voices.Count && (!_usedBars[barI]); voiceI++)
                {
                    Voice voice = bar.Voices[voiceI];
                    for (int i = 0; i < voice.Beats.Count; i++)
                    {
                        var b = voice.Beats[i];
                        if (!b.IsRest)
                        {
                            _usedBars[barI] = true;
                        }
                    }
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            OnPropertyChangedExplicit(propertyName);
        }
        protected virtual void OnPropertyChangedExplicit(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum TrackType
    {
        Default,
        ElectricGuitar,
        BassGuitar,
        Drums,
        Piano
    }
}
