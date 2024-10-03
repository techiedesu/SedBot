module SedBot.Json.SedNetTreeTypeBuilder

open System
open System.Collections.Generic
open Microsoft.FSharp.Reflection
open SedBot.Common.TypeExtensions

type Node =
    { Parents: HashSet<Node>
      Children: HashSet<Node>
      Name: string
      Type: Type }

    static member Empty =
        { Parents = HashSet()
          Children = HashSet()
          Name = null
          Type = null }

let buildTypeTree (rootType: Type) =
    let nodes = HashSet()
    let rootNode = { Node.Empty with Type = rootType }

    let rec loop (loopNode: Node) =
        if FSharpType.IsRecord loopNode.Type then
            for field in FSharpType.GetRecordFields(loopNode.Type) do
                handleNodeAux field.Name field.PropertyType loopNode

        elif FSharpType.IsUnion loopNode.Type then
            for case in FSharpType.GetUnionCases(loopNode.Type) do
                for caseField in case.GetFields() do
                    handleNodeAux caseField.Name caseField.PropertyType loopNode

        elif loopNode.Type.IsArray then
            let elType = loopNode.Type.GetElementType()
            handleNodeAux null elType loopNode

    and handleNodeAux fieldName fieldType (parentNode: Node) =
        let node = nodes |> Seq.tryFind ^ fun n -> n.Type = fieldType && n.Name = fieldName

        match node with
        | None ->
            let node =
                { Node.Empty with
                    Name = fieldName
                    Type = fieldType }

            % node.Parents.Add(parentNode)
            % parentNode.Children.Add(node)
            % nodes.Add(node)
            loop node
        | Some node -> % parentNode.Children.Add(node)

    % nodes.Add(rootNode)
    loop rootNode
    nodes, rootNode

let treeTypeWalker (path: string) (node: Node) =
    node.Children |> Seq.find (_.Name >> (=) path)

let treeTypeWalkerList (path: string list) (root: Node) =
    let rec loop currentNode =
        function
        | [] -> currentNode
        | head :: tail -> currentNode |> treeTypeWalker head |> swap loop tail

    loop root path
