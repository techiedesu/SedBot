module SedBot.Json.SedNetTreeTypeBuilder

open System
open System.Collections.Generic
open System.Diagnostics
open Microsoft.FSharp.Reflection
open SedBot.Common.TypeExtensions

let [<Literal>] ArrayInternalType = "__$type"

/// Ignores return value. Usable for chain APIs
let inline (~%) x = ignore x

[<StructuredFormatDisplay("{DisplayString}")>]
type Node = {
    Parents: HashSet<Node>
    Children: HashSet<Node>
    Name: string
    Type: Type
} with
    static member Empty = {
        Parents = HashSet()
        Children = HashSet()
        Name = ""
        Type = null
    }

    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    member this.DisplayString =
        $"{this.Name} - {this.Type.Name}"

let buildTypeTree (rootType: Type) =
    let nodes = HashSet()
    let root = {
        Node.Empty with
            Type = rootType
    }

    let rec loop (loopNode: Node) =
        let loopType = loopNode.Type
        if FSharpType.IsRecord loopType then
            let recordFields = FSharpType.GetRecordFields(loopType)
            for field in recordFields do
                let fieldName = field.Name
                let fieldType = field.PropertyType
                let fieldTypeNode = nodes |> Seq.tryFind ^ fun n -> n.Type = fieldType && n.Name = fieldName

                match fieldTypeNode with
                | None ->
                    let newNode = {
                        Node.Empty with
                            Name = fieldName
                            Type = fieldType
                    }

                    %newNode.Parents.Add(loopNode)
                    %loopNode.Children.Add(newNode)
                    %nodes.Add(newNode)

                    loop newNode
                | Some currentTypeNode ->
                    %loopNode.Children.Add(currentTypeNode)

        elif FSharpType.IsUnion loopType then
            let unionCases = FSharpType.GetUnionCases(loopType)
            for case in unionCases do
                let caseFields = case.GetFields()
                for caseField in caseFields do
                    let caseFieldNode = nodes |> Seq.tryFind ^ fun n -> n.Type = caseField.PropertyType && n.Name = caseField.Name

                    match caseFieldNode with
                    | None ->
                        let newCaseFieldNode = {
                            Node.Empty with
                                Name = caseField.Name
                                Type = caseField.PropertyType
                        }
                        %newCaseFieldNode.Parents.Add(loopNode)
                        %loopNode.Children.Add(newCaseFieldNode)
                        %nodes.Add(newCaseFieldNode)
                        loop newCaseFieldNode
                    | Some caseFieldNode ->
                        %loopNode.Children.Add(caseFieldNode)
        elif loopType.IsArray then
            let elType = loopType.GetElementType()
            let elNode = nodes |> Seq.tryFind ^ fun n -> n.Type = elType && n.Name = ArrayInternalType
            match elNode with
            | None ->
                let newElNode = {
                    Node.Empty with
                        Name = ArrayInternalType
                        Type = elType
                }
                %newElNode.Parents.Add(loopNode)
                %loopNode.Children.Add(newElNode)
                %nodes.Add(newElNode)
                loop newElNode
            | Some elNode ->
                %loopNode.Children.Add(elNode)

    %nodes.Add(root)
    loop root
    nodes, root

let treeTypeWalker (path: string) (node: Node) =
    node.Children |> Seq.find (_.Name >> (=) path)

let treeTypeWalkerList (path: string list) (root: Node) =
    let rec loop currentNode = function
        | [] -> currentNode
        | head :: tail ->
            currentNode
            |> treeTypeWalker head
            |> swap loop tail

    loop root path
