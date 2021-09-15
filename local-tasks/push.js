const
    gulp = requireModule("gulp"),
    { ls, FsEntities } = require("yafs"),
    path = require("path"),
    nugetPush = requireModule("nuget-push"),
    { packageDir } = require("./config");

gulp.task("push", async () => {
    const toPush = await findPackage("PeanutButter.TrayIcon");
    await nugetPush(toPush);
});

async function findPackage(id) {
    const files = await ls(packageDir, {
        match: new RegExp(`${id}.*\\.nupkg`),
        fullPaths: true,
        entities: FsEntities.files
    });
    return files.sort().reverse()[0];
}