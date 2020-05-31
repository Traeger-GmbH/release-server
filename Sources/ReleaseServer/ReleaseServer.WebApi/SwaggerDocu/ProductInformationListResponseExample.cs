using System.Collections.Generic;
using ReleaseServer.WebApi.Models;
using Swashbuckle.AspNetCore.Filters;

public class ProductInformationListResponseExample : IExamplesProvider<ProductInformationListResponse>
{
    public ProductInformationListResponse GetExamples()
    {
        return new ProductInformationListResponse
        {
            ProductInformation = new List<ProductInformationResponse>
            {
                new ProductInformationResponse
                {
                    Identifier = "softwareX",
                    Version = "1.0",
                    Os = "debian",
                    Architecture = "amd64",
                    ReleaseNotes = new Dictionary<string, List<ChangeSet>>
                    {
                        {"de", new List<ChangeSet>
                            {
                                new ChangeSet
                                {
                                    Platforms = new List<string>{"windows/any", "linux/rpi"},
                                    Added = new List<string>{"added de 1", "added de 2"},
                                    Fixed = new List<string>{"fix de 1", "fix de 2"},
                                    Breaking = new List<string>{"breaking de 1", "breaking de 2"},
                                    Deprecated = new List<string>{"deprecated de 1", "deprecated de 2"}
                                }
                            }
                        },
                        {"en", new List<ChangeSet>
                            {
                                new ChangeSet
                                {
                                    Platforms = new List<string>{"windows/any", "linux/rpi"},
                                    Added = new List<string>{"added en 1", "added en 2"},
                                    Fixed = new List<string>{"fix en 1", "fix en 2"},
                                    Breaking = new List<string>{"breaking en 1", "breaking en 2"},
                                    Deprecated = new List<string>{"deprecated en 1", "deprecated en 2"}
                                },
                                new ChangeSet
                                {
                                    Platforms = null,
                                    Added = new List<string>{"added en 3"},
                                    Fixed = new List<string>{"fix en 3"},
                                    Breaking = new List<string>{"breaking en 3"},
                                    Deprecated = new List<string>{"deprecated en 3"}
                                }
                            }
                        }
                    }
                },
                new ProductInformationResponse
                {
                    Identifier = "softwareX",
                    Version = "1.1-beta",
                    Os = "debian",
                    Architecture = "amd64",
                    ReleaseNotes = new Dictionary<string, List<ChangeSet>>
                    {
                        {"de", new List<ChangeSet>
                            {
                                new ChangeSet
                                {
                                    Platforms = new List<string>{"windows/any"},
                                    Added = new List<string>{"added de 1", "added de 2"},
                                    Breaking = new List<string>{"breaking de 1", "breaking de 2"},
                                    Deprecated = new List<string>{"deprecated de 1", "deprecated de 2"}
                                }
                            }
                        },
                        {"en", new List<ChangeSet>
                            {
                                new ChangeSet
                                {
                                    Added = new List<string>{"added en 1", "added en 2"},
                                    Fixed = new List<string>{"fix en 1", "fix en 2"},
                                    Deprecated = new List<string>{"deprecated en 1", "deprecated en 2"}
                                },
                                new ChangeSet
                                {
                                    Platforms = null,
                                    Added = new List<string>{"added en 3"},
                                    Breaking = new List<string>{"breaking en 3"},
                                    Deprecated = new List<string>{"deprecated en 3"}
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}