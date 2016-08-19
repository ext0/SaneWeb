# SaneWeb
<h3>What is SaneWeb?</h3>
<p>Ever used ASP.NET? SaneWeb is a prototype friendly, lighter version of ASP.NET, fully fledged with native data persistence, highly customizable HTTP endpoints, all in a clean and straightforward C# environment.</p>
<h3>Supported features:</h3>
<ul>
<li>HTML 5</li>
<li>JavaScript</li>
<li>CSS</li>
<li>Extensibility</li>
<li>Lightweight</li>
<li>Very customizable</li>
<li>Programmatically populated templates</li>
<li>GET/POST API requests</li>
<li>SQLite backed database storage</li>
<li>Cached data models</li>
<li>Easy model based database access</li>
</ul>
<h3>What SaneWeb isn't</h3>
<p>SaneWeb isn't a "pick up and go" web server for running up your blog. This is designed as a flexible tool for backend web developers to create quick API endpoints with real data backing, or to run websites that rely on a very customizable and responsive backend. I personally have used SaneWeb for a production-ready website that required split second response times and quick data access to much success.</p>

<h1><b>This setup guide is very outdated! A new one will be written up soon!</b></h1>
<h2>Basic setup</h2>
SaneWeb is a C# library that handles basic web server functionality. Upon compiling, reference the DLL in your project to begin using SaneWeb. After you have referenced the library, there are a few files that must be created. You will need a folder called `Resources` in your solution, as well as a folder called `View`. `Resources` will be used for storing the `ViewStructure` of SaneWeb, whereas `View` will be used to store publically accessible resources (HTML, CSS, JS, etc). Inside of the `Resources` directory, create an XML file called `ViewStructure.xml` (this is arbritrary, and can be modified, but for the sake of this guide, we will be using this). This file contains information on what files will be publically accessible, how they are accessed, and the content-type of each. The following XML data is a sample.
```XML
<?xml version="1.0" encoding="utf-8" ?>
<view>
  <resource path="Home.html" location="SaneWebHost.View.Home.html" content-type="text/html" situational="homepage"></resource>
  <resource path="bootstrap.min.css" location="SaneWebHost.View.bootstrap.min.css" content-type="text/css"></resource>
  <resource path="Images/bird.jpg" location="SaneWebHost.View.Images.bird.jpg" content-type="image/jpeg"></resource>
  <resource path="404.html" location="SaneWebHost.View.404.html" content-type="text/html" situational="404"></resource>
</view>
```
So, each resource value has a few important attributes. Path is the viewable path that will be used from browsers to access the requested resource. For example, `"Home.html"` will be accessible at `domain/Home.html`, and `"Images\bird.jpg"` will be accessible at `domain/Images/bird.jpg`. The location attribute is a bit more complicated. This is the <b>embedded resource location</b> of the resource. Note that the location consists of the default namespace, then the directory with path seperators replaced with periods. So, if your default namespace is `MyWebProject`, and the requested resource is in `View/Login/Help.html`, then the embedded resource location would be `MyWebProject.View.Login.Help.html`. The content-type is the MIME type of the resource. If you aren't sure what this is, use `text/html` for web pages, `image/jpeg` for image files, `text/javascript` for javascript, and `text/css` for stylesheets. The `situational` attribute is for special use case files, such as 404 and homepages. Current situational types are `404` and `homepage`.
All of these resource files should be located in the `View` folder, and be marked with the Build Action of `Embedded Resource`.
Now that the View structure is complete, we can move onto the actual startup of SaneWeb. Below is a sample code for starting the server with basic configuration.
```C#
using SaneWeb.Controller;
using SaneWeb.Data;
using SaneWeb.Resources;
using SaneWeb.Web;

namespace SaneWebHost //this will be implicit for the rest of the examples
{
  public static class WebServer
  {
    static void Main(String[] args)
    {
      SaneServer ws = new SaneServer(
        (Utility.fetchFromResource(true, Assembly.GetExecutingAssembly(), "SaneWebHost.Resources.ViewStructure.xml")),
        "Database\\SaneDB.db",
        "http://+:80/");
      ws.run();
      Console.WriteLine("Webserver running! Press any key to stop...");
      Console.ReadKey();
      ws.stop();
    }
  }
}
```
<h2>API controller</h2>
SaneWeb uses a simple API controller that allows for simple creation of new API calls without worrying about backend control, while giving you access to low-end HTTP controls at the same time. You can choose to ignore the underlying HTTP layer (handling status codes, etc) if you would just like a quick & easy solution, but it's there if necessary.
Each API call must be declared in a static class, with methods defined with the `Controller` attribute along with the URL to where the API will be called from. For example, the following API call with be called through `domain/add/?num1=0&num2=0` and also pass the body (if supplied from a POST request) to the method. Then, the data returned from this request will be delivered to the client.
```C#
using SaneWeb.Resources.Attributes;

public static class Controller
{
  [Controller("~/add/")]
  public static String test(HttpListenerContext context, String body, String num, String num2)
  {
    try
    {
      int numa = int.Parse(num);
      int numb = int.Parse(num2);
      return (numa + numb).ToString();
    }
    catch
    {
      return "Invalid number syntax!";
    }
  }
}
```
<p>Now, the API call is done! Easy as that! :)</p>
<h2>Data access</h2>
Now, lets say you want to store information in a database, that can be created/accessed/modified through API calls. SaneWeb makes this incredibly simple. However, before we get to hooking the API call up to something, we need to create a datatype that will be stored. In SaneWeb, these are called `Models`. Models are object types that will be used to create database tables and entries, and allow for simple interfacing with databases without having to get down and dirty with direct database access.
So, lets make a model! For this particular example, we will make a tool for handling an Alpaca farm. Our `Alpaca` object will have three fields, `Name`, `Age`, and `Description`.
```C#
using SaneWeb.Data;
using SaneWeb.Resources.Attributes;

[Table("Alpacas")]
public class Alpaca : Model<Alpaca>
{
  [DatabaseValue("name", 64)]
  public String name { get; set; }

  [DatabaseValue("age", 4)]
  public String age { get; set; }

  [DatabaseValue("description", 128)]
  public String description { get; set; }

  public Alpaca() : base()
  {
  
  }
  
  public Alpaca(String name, String age, String description) : base()
  {
    this.name = name;
    this.age = age;
    this.description = description;
  }
}
```
So, here we have the definition for `Alpaca`, which contains the `TableAttribute` that points to the table `Alpacas`. This table will be automatically created/opened when you load the model. You may also notice that the Model implements `Model<Alpaca>`. Note that every field is prefixed with the DatabaseValue attribute. In SaneWeb, all values in databases are stored as Strings. While this is a limitation created by maintaining simplicity and ease of access, it doesn't need to limit what kind of data you would like to store. Objects can be stored in databases if serialized to JSON, and numbers can simply be parsed upon being retreived from the database. The DatabaseValue attribute has two parameters, the column name (arbitrary, this is handled by the SaneWeb backend) and the max length of the value. Now, we have two constructors; however, only one is necessary. Every `Model` object must have a constructor with no parameters that extends the `base` constructor. You can add constructors more if you are going to be creating objects yourself and need to input values without reflection.
Now that we have our type declaration, we need to add it to the server! We can perform this by loading the model with the following code.
```C#
SaneServer ws = new SaneServer(...);
ListDBHook<Alpaca> db = ws.loadModel<Alpaca>();
```
What this gives us is an interace for us to load and update data in the database. There are two methods in the `ListDBHook<Alpaca>`, `getData(bool)` and `update()`. Calling `getData(bool)` will return a list of objects stored in the database, or an empty list if the table is empty. The argument determines whether or not to fetch new data from the database or to use the cached data. As a rule of thumb, use `getData(true)` when you load the table for the first time in a session, and `getData(false)` for existing data. Let's add some objects to the database via Models! The following code will add a new `Alpaca` to the database.
```C# 
ListDBHook<Alpaca> db = ws.loadModel<Alpaca>();
List<Alpaca> alpacas = db.getData(true);
alpacas.Add(new Alpaca("Hernandez", "63", "Gray and angry."));
alpacas.update();
```
Calling the update method will push any changes made to the database, including additions, modifications, and removals. This method is irreversible, any data lost through this call will be gone permanently.
For our sample application, we will want to be able to access this `ListDBHook` from anywhere, so we will store it in a `static` variable inside of the main entry of our program. For this example, that is `WebServer.alpacaDB`.
Now that we have a database interface to easily add and access `Alpaca` objects from our database, we can start making some API calls to modify this data.
The following `Controller` code will give some basic functionality to your web application. Keep in mind this assumes you have the `Newtonsoft.Json` NuGet package installed!
```C#
using SaneWeb.Resources.Attributes;
using Newtonsoft.Json;

public static class Controller
{
  [Controller("~/addAlpaca/")]
  public static String test(HttpListenerContext context, String body, String name, String age, String description)
  {
    List<Alpaca> alpacas = WebServer.alpacaDB.getData(false);
    Alpaca alpaca = new Alpaca(name, age, description));
    alpacas.add(alpaca);
    WebServer.alpacaDB.update();
    return JsonConvert.SerializeObject(alpaca);
  }
  
  [Controller("~/getAlpaca/")]
  public static String test(HttpListenerContext context, String body, String name)
  {
    List<Alpaca> alpacas = WebServer.alpacaDB.getData(false);
    foreach (Alpaca alpaca in alpacas)
    {
      if (alpaca.name.Equals(name))
      {
        return JsonConvert.SerializeObject(alpaca);
      }
    }
    return "No alpaca under that name!";
  }
  
  [Controller("~/increaseAge/")]
  public static String test(HttpListenerContext context, String body, String name)
  {
    List<Alpaca> alpacas = WebServer.alpacaDB.getData(false);
    bool flag = false;
    foreach (Alpaca alpaca in alpacas)
    {
      if (alpaca.name.Equals(name))
      {
        alpaca.age++;
        flag = true;
        break;
      }
    }
    if (flag)
    {
      WebServer.alpacaDB.update();
      return "Increased age successfully!";
    }
    else
    {
      return "No alpaca under that name!";
    }
  }
}
```
So, now we have a controller, a model, and a webserver. To complete the example, all we have to do is hook them in and run! The following code will hook in the controller and load the model into SaneServer.
```C#
using SaneWeb.Controller;
using SaneWeb.Data;
using SaneWeb.Resources;
using SaneWeb.Web;

public static class WebServer
{
  public static ListDBHook<Alpaca> alpacaDB;
  
  static void Main(String[] args)
  {
    SaneServer ws = new SaneServer(
      (Utility.fetchFromResource(true, Assembly.GetExecutingAssembly(), "SaneWebHost.Resources.ViewStructure.xml")),
      "Database\\SaneDB.db",
      "http://+:80/");
    alpacaDB = ws.loadModel<Alpaca>();
    ws.addController(typeof(Controller));
    ws.run();
    Console.WriteLine("Webserver running! Press any key to stop...");
    Console.ReadKey();
    ws.stop();
  }
}
```
Now, running the project will load up a web server! Write up a quick HTML page and some JavaScript to query the API with AJAX requests and you're good to go!
