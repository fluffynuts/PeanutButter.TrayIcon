const path = require("path"),
  gulp = requireModule("gulp-with-help"),
  lsR = requireModule("ls-r"),
  throwIfNoFiles = requireModule("throw-if-no-files"),
  opts = {
    parserOptions: {},
    builderOptions: {
      headless: true,
      renderOpts: {
        pretty: true
      }
    }
  },
  editXml = require("gulp-edit-xml");

gulp.task(
  "update-tempdb-runner-files",
  "Updates the files section of PeanutButter.TempDb.Runner/Package.nuspec to include all Release binaries",
  () => {
    const project = "source/TempDb/PeanutButter.TempDb.Runner",
      nuspec = `${project}/Package.nuspec`;
    return gulp
      .src(nuspec)
      .pipe(throwIfNoFiles(`Nuspec not found at: ${nuspec}`))
      .pipe(
        editXml(xml => {
          const releaseFolder = `${project}/bin/Release`,
            projectFullPath = path.resolve(project),
            files = lsR(releaseFolder)
              .map(p => p.replace(projectFullPath, ""))
              .map(p => p.replace(/^\\/, ""));
          if (files.filter(p => p.match(/\.dll$/)).length === 0) {
            throw new Error(`No assemblies found under ${releaseFolder}`);
          }

          xml.package.files = [{ file: [] }];
          const fileNode = xml.package.files[0].file;
          fileNode.push({
            $: {
              src: "icon.png",
              target: ""
            }
          })
          files.forEach(relPath => {
            fileNode.push({
              $: {
                src: relPath,
                target: targetFor(relPath)
              }
            });
          });

          return xml;
        }, opts)
      )
      .pipe(gulp.dest(project));
  }
);

function targetFor(relPath) {
  let next = false;
  const parts = relPath.split(path.sep),
    targetFramework = parts.reduce((acc, cur) => {
      if (cur === "Release") {
        next = true;
        return acc;
      }
      if (next) {
        next = false;
        return cur;
      } else {
        return acc;
      }
    }, null);
  if (!targetFramework) {
    console.log({
      parts
    });
    throw new Error(
      `Can't determine target framework for path: ${relPath} (should be after 'Release')`
    );
  }
  return `lib/${targetFramework}`;
}
