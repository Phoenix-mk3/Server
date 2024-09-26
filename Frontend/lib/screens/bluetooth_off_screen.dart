import 'package:flutter/material.dart';
import 'package:flutter_blue_plus/flutter_blue_plus.dart';

class BluetoothOffScreen extends StatelessWidget {
  final BluetoothAdapterState adapterState;

  const BluetoothOffScreen({Key? key, required this.adapterState}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Bluetooth is Off'),
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text(
              'Bluetooth is currently ${adapterState == BluetoothAdapterState.off ? "off" : "unavailable"}',
            ),
            ElevatedButton(
              onPressed: () {
                // Here you can show a message to the user to turn on Bluetooth
              },
              child: const Text('Turn on Bluetooth'),
            ),
          ],
        ),
      ),
    );
  }
}
