![ektron cms development](http://dev.ektron.com/images/headerlogo.png)

#Ektron Contribution Project:
This project was designed to add functionality and ease-of-use to Ektron's CMS Framework. 

##What's new:
**Distributed caching and session** implementation using [Redis](http://redis.io/). Redis is a blisteringly fast advanced key-value store that supports transactions. In the dot net community, when people think about Redis they tend to think about Stack Exchange. Their open source contributions and use of Redis on [Stackoverflow.com](http://stackoverflow.com) has shed a lot of light on this wounderful product. Our implementations include:

  - **API Cache Provider**
  - **Session Provider**
  - **Output Cache Provider**

##Requirements
Ektron Version 8.6 or above. A thorough code review at Ektron was done to insure that all of the appropriate classes in Ektron's Framework were serializable.

## API Cache Provider
Ektron version 8.5 and above include the Framework APIs. If configured, the manager classes in the Framework API makes calls into the caching layer to improve performance. The Ektron.Contrib implementation supports transactions against the cache, providing a consistent data experience.

## Session Provider
With the changes in 8.6 release to serialize the appropriate classes implementations no longer have to utilize sticky sessions. Using sql server session is one solution but doesn't lend itself well to sites that need that extra level of performance. This is where Ektron.Contrib shines. Using Redis as a backing store, you can configure a custom session provider and watch your site increase its response time.   

## Output Cache  Provider
You can move away from IIS Output cache to a distributed cache solution. The advantage is that each web server has the same presentation layer output cached. The disadvantage is that it will require a network hop.
 
## Getting Started
Download the code and review the sample site. It includes an example of each implementation.

 * Install and configure Redis on a linux server or acquire service. Windows version can be used for development.
 * Download the code
 * Add licensed Ektron dlls into a lib folder at the src level
 * Build project
 * Configure your site


## Find out More
 * Follow [@billcava](http://twitter.com/billcava)

## Future Roadmap
1. AppFabic
2. Amazon Elasticache
3. Memcache
4. Microsoft Azure


## OSS Libraries used

Ektron contrib uses several open source libraries. Each library is released under its respective licence:

  - [Redis](http://redis.io)
  - [ServiceStack](http://www.servicestack.net/)
  - [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
  - [ASP.Net Microsoft] (http://aspnet.codeplex.com/)
  - [WebActivator](https://github.com/davidebbo/WebActivator)
  - [Modernizr](https://github.com/Modernizr/Modernizr)


## Core Team
 - [Bill Cava](https://github.com/ektron)
 - [Lance Farquhar](https://github.com/lfarquhar)
 
## Contributors 
A big thanks to GitHub. We are looking for contributors.

***

