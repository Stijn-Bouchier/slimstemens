﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleRound : GameRound
{
    //private static readonly PuzzleQuestion[] Questions = new PuzzleQuestion[]
    //    {
    //        new PuzzleQuestion() { Answers =  new[] 
    //                {
    //                    new PuzzleAnswer() { Answer = "Thomas", TimeReward = 30, Words = new [] 
    //                            {
    //                                "Ijstaart",
    //                                "Knuffelbeer",
    //                                "Philips",
    //                                "Gele rugzak",
    //                            } },
    //                    new PuzzleAnswer() { Answer = "Dublin", TimeReward = 30, Words = new [] 
    //                            {
    //                                "Stiletto",
    //                                "Stijn",
    //                                "Het Leven Zoals Het Is: De Zoo",
    //                                "Guinness",
    //                            } },
    //                    new PuzzleAnswer() { Answer = "Eerste Communie", TimeReward = 30, Words = new [] 
    //                            {
    //                                "6 jaar",
    //                                "Lammetje",
    //                                "Lentefeest",
    //                                "Hostie",
    //                            } },
    //                }},
    //        new PuzzleQuestion() { Answers =  new[] 
    //                {
    //                    new PuzzleAnswer() { Answer = "Google", TimeReward = 30, Words = new [] 
    //                            {
    //                                "YouTube",
    //                                "Pixel",
    //                                "Allo",
    //                                "Hangouts",
    //                            } },
    //                    new PuzzleAnswer() { Answer = "Merijn", TimeReward = 30, Words = new [] 
    //                            {
    //                                "Merel",
    //                                "Chiro",
    //                                "Basgitaar",
    //                                "Kattenfilmpjes",
    //                            } },
    //                    new PuzzleAnswer() { Answer = "Agalev", TimeReward = 30, Words = new [] 
    //                            {
    //                                "Politieke partij",
    //                                "Anders Gaan Leven",
    //                                "Vogels",
    //                                "Groen",
    //                            } },
    //                }},
    //        new PuzzleQuestion() { Answers =  new[] 
    //                {
    //                    new PuzzleAnswer() { Answer = "Straatje Zonde Einde", TimeReward = 30, Words = new [] 
    //                            {
    //                                "Jeugdhuis",
    //                                "Lembeke",
    //                                "Tiny Mix Tapes",
    //                                "Gesloten",
    //                            } },
    //                    new PuzzleAnswer() { Answer = "College Onze Lieve Vrouw ten Doorn", TimeReward = 30, Words = new [] 
    //                            {
    //                                "Eeklo",
    //                                "Rock Oasis",
    //                                "Mevrouw Martens",
    //                                "Fietsenstalling",
    //                            } },
    //                    new PuzzleAnswer() { Answer = "Sarah", TimeReward = 30, Words = new [] 
    //                            {
    //                                "\"Ma Fack\"",
    //                                "Chocola",
    //                                "Sleidinge",
    //                                "3 September",
    //                            } },
    //                }},
    //    };

    private TeamData[] _teams;
    private int _currentQuestionIndex;

    private int _currentCorrectAnswersCount;
    private int _currentTeamIndex;
    private List<int> _currentQuestionTeamsPlayedIndeces;

    private int _currentQuestionTeamIndex;
    private List<int> _roundTeamsPlayedIndeces;

    private Action<float> _timer;

    private PuzzleViewController _view;

    private Action _onWaitingForNextQuestion = () => { };
    private Action _onWaitingForNextPlayer = () => { };
    private PuzzleQuestion[] _questions;

    public event Action OnWaitingForNextQuestionPrompt
    {
        add
        {
            _onWaitingForNextQuestion -= value;
            _onWaitingForNextQuestion += value;
        }
        remove
        {
            _onWaitingForNextQuestion -= value;
        }
    }

    public event Action OnWaitingForNextPlayer
    {
        add
        {
            _onWaitingForNextPlayer -= value;
            _onWaitingForNextPlayer += value;
        }
        remove
        {
            _onWaitingForNextPlayer -= value;
        }
    }

    private TeamData CurrentTeam
    {
        get{ return _teams[_currentTeamIndex]; }
    }

    private PuzzleQuestion CurrentQuestion
    {
        get{ return _questions[_currentQuestionIndex]; }
    }

    #region implemented abstract members of GameRound
    public override void Start(TeamData[] teams, Question[] questions)
    {
        _questions = questions as PuzzleQuestion[];
        _currentQuestionIndex = -1;
        _currentQuestionTeamIndex = -1;
        _currentTeamIndex = -1;
        _teams = teams;
        _roundTeamsPlayedIndeces = new List<int>(_teams.Length);
        _currentQuestionTeamsPlayedIndeces = new List<int>(_teams.Length);

        _timer = UpdateCurrentTeamTime;

        _view = GameObject.FindObjectOfType<PuzzleViewController>();
        _view.SetTeamData(_teams);
        _view.SetController(this);

        _onWaitingForNextQuestion();
    }
    public override string SceneName
    {
        get
        {
            return "Puzzle";
        }
    }
    #endregion

    public void NextQuestion()
    {
        _currentCorrectAnswersCount = 0;
        _currentQuestionTeamsPlayedIndeces.Clear();

        _currentQuestionTeamIndex = GetNextTeamIndex(_roundTeamsPlayedIndeces, _currentQuestionTeamIndex);

        if (_currentQuestionTeamIndex != -1)
        {
            ++_currentQuestionIndex;

            _currentTeamIndex = _currentQuestionTeamIndex;

            // Show question
            _view.SetAnswers(CurrentQuestion.GetTimeRewards(), CurrentQuestion.GetAnswers());
            _view.SetPuzzleWords(CurrentQuestion.GetWords());

            StartTimer();
        }
        else
        {
            GameManager.NextRound();
        }
    }

    public void StartTimer()
    {
        _view.SetActiveTeam(_currentTeamIndex, true);
        TimeManager.Instance.AddTimer(_timer);
    }

    private void StopTimer()
    {
        _view.SetActiveTeam(_currentTeamIndex, false);
        TimeManager.Instance.RemoveTimer(_timer);
    }

    private void UpdateCurrentTeamTime(float timeDelta)
    {
        CurrentTeam.Time -= timeDelta;
    }

    private void EndQuestion()
    {
        StopTimer();

        // Show result
        _onWaitingForNextQuestion();
    }

    public void ShowAnswer(int answerIndex)
    {
        // Show answer
        _view.ShowAnswer(answerIndex, CurrentQuestion.GetAnswerWordIndeces(answerIndex), false);
    }

    public void CorrectAnswer(int answerIndex)
    {
        // Show answer
        _view.ShowAnswer(answerIndex, CurrentQuestion.GetAnswerWordIndeces(answerIndex), true);

        CurrentTeam.Time += CurrentQuestion.Answers[answerIndex].TimeReward;

        if (++_currentCorrectAnswersCount == CurrentQuestion.Answers.Length)
        {
            CurrentTeam.Time = Mathf.CeilToInt(CurrentTeam.Time);
            EndQuestion();
        }
    }

    private int GetNextTeamIndex(List<int> teamsPlayed, int currentTeamIndex)
    {
        if (currentTeamIndex != -1)
        {
            teamsPlayed.Add(currentTeamIndex);
        }

        if (teamsPlayed.Count == _teams.Length)
        {
            return -1;
        }

        int nextTeamIndex = -1;

        for (int teamIndex = 0; teamIndex < _teams.Length; teamIndex++)
        {
            if (teamsPlayed.Contains(teamIndex))
            {
                continue;
            }
            if (nextTeamIndex == -1 || _teams[teamIndex].Time < _teams[nextTeamIndex].Time)
            {
                nextTeamIndex = teamIndex;
            }
        }
        return nextTeamIndex;
    }

    public void TeamPassed()
    {
        CurrentTeam.Time = Mathf.CeilToInt(CurrentTeam.Time);

        _currentTeamIndex = GetNextTeamIndex(_currentQuestionTeamsPlayedIndeces, _currentTeamIndex);

        if (_currentTeamIndex == -1)
        {
            EndQuestion();
        }
        else
        {
            _onWaitingForNextPlayer();
            StopTimer();
        }
    }
}