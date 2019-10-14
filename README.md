# BirdOrNot.net - Is it a bird or not?

As a technical person and developer, I'm a big fan of [XKCD](https://xkcd.com/) which makes entertaining comics about our industry. A couple of years ago I stumbled upon this one:

![XKCD](https://imgs.xkcd.com/comics/tasks.png)

Turns out that it was published September 2014, almost exactly 5 years ago, the time the team would need to solve the problem according to the comic. So, I thought, this should be solved now, right? And it turns out it did!

## Today I'm proud to announce: BirdOrNot.net

![BirdOrNot.net](docs/images/BirdOrNotNet_Form.png)

BirdOrNot.net uses the object detection capability in [Azure Cognitive Services](https://bit.ly/azure-cog-computer-vision) to find out if the image contains a bird and, in some cases, it even finds out what species it is. I didn't have to hire a team of researchers for five years, instead Microsoft did and made the result available as a service in Azure.

Using that specific service, I was able to go from idea to working concept in under an hour, with a few more hours spent on CSS :)

Below you can see a few samples, feel free to try it out yourself on the site:
[https://birdornot.net/](https://birdornot.net/)

![BirdOrNot.net](docs/images/BirdOrNotNet_Owls.png)

It builds on the default object detection model provided by Azure Cognitive Services, which can detect anything from buildings, vehicles to animals. It does a very good job of detecting birds, but as it's not specialized on birds, there will be cases where it makes mistakes. Still, shows how the power of AI and ML can be used by everyone who knows how to call a webservice.

If you boil it down, these are the few lines that powers the site:
```csharp
var imageAnalysis = await computerVisionClient.AnalyzeImageAsync(url, new List<VisualFeatureTypes>
{
    VisualFeatureTypes.Objects
});

var isBird = imageAnalysis.Objects.Any(x => x.ObjectProperty.Equals("bird"));

Console.WriteLine(isBird ? "It is a bird." : "It is not a bird.");
```

## Behind the scenes
The source code is fully available at GitHub, feel free to dig around to learn how it works:
For those of you that are interested, here comes a brief explanation on the setup.
Object recognition
The service is using Azure Cognitive Services (TODO) to analyse the image. It  

## Interested in even more details?

If you or your company wants to know more about services like this, I deliver a session/Workshop called "[Democratizing AI with Azure Cognitive Services](http://bit.ly/peterorneholm-democratizing-ai)" which shows the potential of Cognitive Services by fun and creative examples. Please drop me an email if you want more details.