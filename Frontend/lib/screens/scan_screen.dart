import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_blue_plus/flutter_blue_plus.dart';

class ScanScreen extends StatefulWidget {
  const ScanScreen({Key? key}) : super(key: key);

  @override
  State<ScanScreen> createState() => _ScanScreenState();
}

class _ScanScreenState extends State<ScanScreen> {
  StreamSubscription<List<ScanResult>>? scanSubscription;

  @override
  void initState() {
    super.initState();
    scanForDevices();
  }

  @override
  void dispose() {
    scanSubscription?.cancel();
    super.dispose();
  }

  void scanForDevices() {
    FlutterBluePlus.startScan(timeout: const Duration(seconds: 10));
    scanSubscription = FlutterBluePlus.scanResults.listen((results) {
      for (ScanResult result in results) {
        print('Found device: ${result.device.remoteId} with name ${result.device.name}');
        // Add logic to connect to the device and control it.
      }
    }, onError: (error) {
      print("Scan error: $error");
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Scan for Devices'),
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            ElevatedButton(
              onPressed: scanForDevices,
              child: const Text('Rescan for devices'),
            ),
            // Add more UI elements to show scan results
          ],
        ),
      ),
    );
  }
}
