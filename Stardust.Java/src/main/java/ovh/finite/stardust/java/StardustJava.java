/*
 * Stardust sample Java app.
 *
 * A plain Java application with a standard main(String[]) entry point.
 * When the JAR is dropped into /bin it can be launched through IKVM
 * from the Stardust shell:
 *
 *   run /bin/StardustJava.jar
 *   run StardustJava
 *   run StardustJava.jar hello stardust
 */

package ovh.finite.stardust.java;

import java.util.Arrays;
import java.time.LocalDateTime;

/**
 * Launches inside Stardust via the IKVM runtime, no compilation step needed.
 */
public class StardustJava {

    public static void main(String[] args) {
        System.out.println("[StardustJava] Hello from a Java app running inside Stardust!");
        System.out.println("[StardustJava] Started at " + LocalDateTime.now());
        if (args.length == 0) {
            System.out.println("[StardustJava] No arguments given.");
        } else {
            System.out.println("[StardustJava] Arguments: " + Arrays.toString(args));
        }
    }
}