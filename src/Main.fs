module Main

open System.Text.RegularExpressions
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open GlobalHelpers
open Components
open Util.Literals
open Util.Types
open Fable.Core
open Fulma
module Node = Node.Api

let render (info: PageInfo) =
    Frame.render info.Title
      (Navbar.root info.NavbarActivePage)
      (info.RenderBody(info))
    |> parseReactStatic
    |> IO.writeFile info.TargetPath

let renderMarkdown pageTitle navbar className targetFullPath content =
    let body =
      div [Class ("markdown " + className); Style [Overflow "hidden"]] [
        Header.render()
        div [Style [MarginTop "1.6rem"]] [
          div [Class "columns"] [
            div [Class "column"] []
            div [Class "column is-two-thirds"] [
                div [
                  Class "content"
                  Style [Margin "5px"]
                  DangerouslySetInnerHTML { __html = parseMarkdown content }] []
              ]
            div [Class "column"] []
          ]
        ]
      ]
    Frame.render pageTitle (Navbar.root navbar) body
    |> parseReactStatic
    |> IO.writeFile targetFullPath

let renderMarkdownFrom pageTitle navbar className fileFullPath targetFullPath =
  let content = IO.readFile fileFullPath
  renderMarkdown pageTitle navbar className targetFullPath content

// let renderDocs() =
//   let pageTitle, header, subheader = "Fable Docs", "Documentation", "Learn how Fable works & how to use it"
//   // Main docs page
//   render
//     { Title = pageTitle
//       TargetPath = Node.path.join(Paths.DeployDir, "docs", "index.html")
//       NavbarActivePage = Navbar.Docs
//       RenderBody = DocsPage.renderBody header subheader }
//   // Docs translated from markdown files in Fable repo
//   let docFiles = Node.fs.readdirSync(!^Node.path.join(Paths.FableRepo, "docs"))
//   for doc in docFiles |> Seq.filter (fun x -> x.EndsWith(".md") && not(x.EndsWith("FAQ.md"))) do
//     let fullPath = Node.path.join(Paths.FableRepo, "docs", doc)
//     let targetPath = Node.path.join(Paths.DeployDir, "docs", doc.Replace(".md", ".html"))
//     renderMarkdownFrom pageTitle Navbar.Docs header subheader "docs" fullPath targetPath
//   printfn "Documentation generated"


let renderFaq() =
  let subheader = "The place to find a quick answer to your questions"
  let fullPath = Node.path.join(Paths.FableRepo, "docs/FAQ.md")
  let targetPath = Node.path.join(Paths.DeployDir, "faq/index.html")
  renderMarkdownFrom "Fable FAQ" Navbar.FAQ "faq" fullPath targetPath
  printfn "FAQ generated"

let renderBlog() =
  let reg = Regex(@"^\s*-\s*title\s*:(.+)\n\s*-\s*subtitle\s*:(.+)\n")
  let pageTitle, header, subheader = "Fable Blog", "Blog", "Read about latest Fable news"
  render
    { Title = "Fable Blog"
      TargetPath = Node.path.join(Paths.DeployDir, "blog", "index.html")
      NavbarActivePage = Navbar.Blog
      RenderBody = BlogIndex.renderBody }  
  let blogFiles = Node.fs.readdirSync(!^Paths.BlogDir)
  for blog in blogFiles |> Seq.filter (fun x -> x.EndsWith(".md") && not(x.EndsWith("index.md"))) do
    let text = Node.path.join(Paths.BlogDir, blog) |> IO.readFile
    let m = reg.Match(text)
    let header, subheader, text =
      if m.Success
      then m.Groups.[1].Value.Trim(), m.Groups.[2].Value.Trim(), text.Substring(m.Index + m.Length)
      else header, subheader, text
    let targetPath = Node.path.join(Paths.DeployDir, "blog", blog.Replace(".md", ".html"))
    renderMarkdown pageTitle Navbar.Blog "blog" targetPath text
  printfn "Blog generated"

let renderHomePage() =
  render
    { Title = "Fable: JavaScript you can be proud of!"
      TargetPath = Node.path.join(Paths.DeployDir, "index.html")
      NavbarActivePage = Navbar.Home
      RenderBody = HomePage.renderBody }
  printfn "Home page generated"

// Run
IO.copy Paths.PublicDir Paths.DeployDir
renderHomePage()
renderBlog()
// renderDocs()
//renderFaq()
