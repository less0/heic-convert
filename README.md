## Summary

*heic-convert* is a small dockerized application that converts HEIC files to JPG. It watches on a configurable folder 
for HEIC files to come in and converts them using ImageMAgick.

## License

*heic-convert* is released under the [MIT License](https://github.com/less0/heic-convert/blob/main/LICENSE).

## Building with `dotnet`

The application can be built and run without Docker, with the `dotnet` command. The .net 8 SDK 
has to be installed, furthermore the `magick` command (ImageMagick) has to be available from the 
command line. From `heic-convert.Application` directory, the application can be built with 

```
dotnet build
```

Afterward the application can be run from the `bin\Debug\net8.0` directory. By default, the 
directories `in`, `out` and `error` are created as subdirectories of the `net8.0` directory. 
Custom directories can be set in `appsettings.json`

| Configuration key | Explanation |
|-|-|
| `WatchDirectory` | The directory the application watches at. |
| `OutDirectory` | The directory the merged PDFs are written to. |
| `ErrorDirectory` | The directory erroneous PDFs are written to. |
| `AddHealthcheckEndpoint` | A boolean value that states whether a healthcheck shall be available at `<host>:8080/health`. |

## Building with Docker

To build and run the application with Docker, Docker has to be installed. ImageMagick will be 
installed to the container during the build process and is not required to be preinstalled. 
From `heic-convert.Application` the image can be built with

```
docker build -t heic-convert .
```

To add files to be merged, it comes handy to have mounted volumes you can write the PDFs to 
and read them after they have been merged. 

## Integration with portainer

Since portainer does not allow custom options when you create a CIFS volume and by default a 
CIFS volume is mounted as *root* without write access for the restricted user that runs the 
app, you can run *heic-convert* on portainer using Stacks. Stacks allow you to create a Docker 
compose file *"on the fly"* and within that create CIFS volumes with a custom UID. 

**Example Stack:**

```
services:
  heic-convert:
    image: less0/heic-convert:latest
    container_name: heic-convert_worker
    volumes:
      - in:/app/in 
      - out:/app/out
    restart: unless-stopped

volumes:
  in:
    driver: local
    driver_opts:
      type: "cifs"
      device: "<input network share>" # e.g. //192.168.0.123/heic-convert/in
      o: "username=<input network share user>,password=<input network share pass>,vers=2.0,uid=1654,gid=1654"
  out:
    driver: local
    driver_opts:
      type: "cifs"
      device: "<output network share>" # e.g. //192.168.0.123/heic-convert/out
      o: "username=<output network share user>,password=<output network share pass>,vers=2.0,uid=1654,gid=1654"
```

Just replace the parts in the angle brackets with the values suitable for your setup. `1654` is the 
UID and GID of the user and group `app` in the image.

## Docker variables

| Variable | |
|-|-|
| `DOTNET_WatchDirectory` | The directory the application watches at. Effectively defaults to `"/app/in"`. |
| `DOTNET_OutDirectory` | The directory the merged PDFs are written to. Effectively defaults to `"/app/out"` |
| `DOTNET_ErrorDirectory` | The directory, erroneous PDFs are written to. Effectively defaults to `"/app/error"` |
| `DOTNET_AddHealthcheckEndpoint` | A boolean value that states whether a healthcheck shall be available at `<host>:8080/health`. |

# Known limitations

- At the moment *heic-convert* might struggle on files being added simultanously, this might result in unexpected results, make sure files are added one after another