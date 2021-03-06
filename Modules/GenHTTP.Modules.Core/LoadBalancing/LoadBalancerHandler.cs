﻿using System;
using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.LoadBalancing
{

    public class LoadBalancerHandler : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private readonly List<(IHandler, PriorityEvaluation)> _Nodes;

        private static readonly Random _Random = new Random();

        #endregion

        #region Initialization

        public LoadBalancerHandler(IHandler parent, List<(IHandlerBuilder, PriorityEvaluation)> nodes)
        {
            Parent = parent;

            _Nodes = nodes.Select(n => (n.Item1.Build(this), n.Item2)).ToList();
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            // get the handlers that share the highest priority
            var priorityGroup = _Nodes.GroupBy(n => n.Item2(request))
                                      .OrderByDescending(n => n.Key)
                                      .FirstOrDefault()?
                                      .Select(n => n.Item1)
                                      .ToList();

            if (priorityGroup != null)
            {
                // let a random one handle the request
                if (priorityGroup.Count > 1)
                {
                    var index = _Random.Next(0, priorityGroup.Count);

                    return priorityGroup[index].Handle(request);
                }
                else
                {
                    return priorityGroup.First()
                                        .Handle(request);
                }
            }

            return null;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => Enumerable.Empty<ContentElement>();

        #endregion

    }

}
