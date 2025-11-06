plugins {
    base
}

allprojects {
    group = "com.example"
    version = "1.0.0"
}

tasks.register("buildAll") {
    dependsOn(
        ":java-module:build",
        //":csharp-module:build",
        //":python-module:build"
    )
}
