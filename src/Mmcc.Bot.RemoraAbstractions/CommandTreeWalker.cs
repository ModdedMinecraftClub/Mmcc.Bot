using System;
using System.Collections.Generic;
using System.Linq;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;

namespace Mmcc.Bot.RemoraAbstractions;

public class CommandTreeWalker
{
    private readonly CommandTree _commandTree;
    
    public CommandTreeWalker(CommandTree commandTree)
        => _commandTree = commandTree;
    
    public void PreOrderTraverseParentNodes(Action<IParentNode> onParentNode) 
        => PreOrderTraverseParentNodes(_commandTree.Root, onParentNode);

    public GroupNode? GetGroupNodeByPath(List<string> path)
    {
        var root = _commandTree.Root;

        var currPathIndex = 0;
        var children = root.Children.OfType<GroupNode>().ToList();
        while (true)
        {
            var matchedChild = children.FirstOrDefault(c => c.Key.Equals(path[currPathIndex]));

            if (matchedChild is null)
                return null;
            
            if (currPathIndex == path.Count - 1)
                return matchedChild;
            
            currPathIndex++;
            children = matchedChild.Children.OfType<GroupNode>().ToList();
        }
    }
    
    public List<string> CollectPath(GroupNode node)
    {
        IEnumerable<string> res = new List<string> { node.Key };
        var parent = node.Parent;
        while (parent is GroupNode groupNode)
        {
            res = res.Prepend(groupNode.Key);
            parent = groupNode.Parent;
        }
        
        return res.ToList();
    }
    
    private void PreOrderTraverseParentNodes(IParentNode parentNode, Action<IParentNode> onNode)
    {
        onNode(parentNode);

        foreach (var childNode in parentNode.Children.OfType<IParentNode>())
        {
            PreOrderTraverseParentNodes(childNode, onNode);
        }
    }
}